// api/durable/websocket.js
import { kv } from '@vercel/kv';

export class WebSocketDurable {
	hamburburSockets = new Map();
	trackerSockets = new Set();
	chatSockets = new Map();

	constructor() {
		this.env = process.env;
	}

	broadcastUsers() {
		const users = Array.from(this.hamburburSockets.values()).map(
			user => ({
				userId: user.userId,
				username: user.username
			})
		);

		const payload = JSON.stringify({ type: 'broadcastUsers', users: users });

		for (const [socket, info] of this.hamburburSockets.entries()) {
			try {
				if (socket.readyState === 1) {
					socket.send(payload);
				}
			} catch (e) {
				this.hamburburSockets.delete(socket);
			}
		}
	}

	async handleMessage(ws, message) {
		const data = JSON.parse(message.toString());
		const type = data.type;

		switch (type) {
			case 'ping':
				ws.send(JSON.stringify({ type: 'pong', timeStamp: Date.now() }));
				break;

			case 'telemetryUpload':
				const telemetryPayload = {
					embeds: [
						{
							title: `Code uploaded to telemetry by ${data.username}`,
							fields: [
								{ name: 'Code', value: data.roomCode || 'N/A' },
								{ name: 'Players In Code', value: String(data.playersInCode || 'N/A') },
								{ name: 'GameMode String', value: data.gameModeString || 'N/A' }
							]
						}
					]
				};

				await fetch(this.env.GC_DEV_WEBHOOK, {
					method: 'POST',
					headers: { 'Content-Type': 'application/json' },
					body: JSON.stringify(telemetryPayload)
				});
				break;

			case 'broadcastData':
				for (const socket of this.hamburburSockets.keys()) {
					try {
						if (socket.readyState === 1) {
							socket.send(JSON.stringify(data));
						}
					} catch (e) {
						this.hamburburSockets.delete(socket);
					}
				}
				break;
		}
	}

	async handleUpgrade(ws, req) {
		const url = new URL(req.url, `http://${req.headers.host}`);

		if (url.pathname === '/websocket') {
			const key = url.searchParams.get('key');
			const userId = url.searchParams.get('userId');
			const username = url.searchParams.get('username');

			if (!key || key !== this.env.SECRET_KEY || !userId || !username) {
				ws.close(4001, 'Unauthorized or Missing Params');
				return;
			}

			this.hamburburSockets.set(ws, { userId, username });

			ws.on('message', (msg) => this.handleMessage(ws, msg));
			ws.on('close', () => {
				this.hamburburSockets.delete(ws);
				this.broadcastUsers();
			});

			this.broadcastUsers();
		}

		if (url.pathname === '/chat-socket') {
			// public chat socket: assign anonymous id like anon-1234
			const anonId = Math.floor(1000 + Math.random() * 9000);
			const username = `anonymous-${anonId}`;

			const info = {
				username,
				connectedAt: Date.now()
			};

			this.chatSockets.set(ws, info);

			const sendToAll = (payloadObj) => {
				const payload = JSON.stringify(payloadObj);
				for (const socket of this.chatSockets.keys()) {
					try {
						if (socket.readyState === 1) {
							socket.send(payload);
						}
					} catch (e) {
						this.chatSockets.delete(socket);
					}
				}
			};

			const broadcastPresence = () => {
				const payload = JSON.stringify({
					type: 'chatPresence',
					count: this.chatSockets.size,
					ts: Date.now()
				});
				for (const socket of this.chatSockets.keys()) {
					try {
						if (socket.readyState === 1) {
							socket.send(payload);
						}
					} catch (e) {
						this.chatSockets.delete(socket);
					}
				}
			};

			// send recent history to this client (last 200 messages, kept ~24h)
			try {
				const history = await kv.lrange('chat:history', -200, -1);
				for (const raw of history) {
					try {
						const msg = JSON.parse(raw);
						ws.send(JSON.stringify(msg));
					} catch (_) {}
				}
			} catch (_) {}

			// send welcome to this client with assigned name
			ws.send(JSON.stringify({
				type: 'chatWelcome',
				username,
				ts: Date.now()
			}));

			// announce join to others
			sendToAll({
				type: 'chatSystem',
				text: `${username} joined the chat`,
				ts: Date.now()
			});
			broadcastPresence();

			ws.on('message', async (msg) => {
				let payload;
				try {
					payload = JSON.parse(msg.toString());
				} catch {
					payload = { type: 'chatMessage', text: msg.toString() };
				}

				if (payload.type !== 'chatMessage') return;
				const text = (payload.text || '').toString().trim();
				if (!text) return;

				const fromInfo = this.chatSockets.get(ws) || info;

				// detect @mentions against current usernames
				const mentions = [];
				const lowerText = text.toLowerCase();
				for (const socket of this.chatSockets.keys()) {
					const inf = this.chatSockets.get(socket);
					if (!inf || !inf.username) continue;
					const uname = inf.username;
					const needle = '@' + uname.toLowerCase();
					if (lowerText.includes(needle)) {
						mentions.push(uname);
					}
				}

				const record = {
					type: 'chatMessage',
					from: fromInfo.username,
					text,
					ts: Date.now(),
					mentions
				};

				sendToAll(record);

				// persist to KV for ~24h
				try {
					await kv.rpush('chat:history', JSON.stringify(record));
					await kv.ltrim('chat:history', -200, -1);
					await kv.expire('chat:history', 60 * 60 * 24);
				} catch (_) {}
			});

			ws.on('close', () => {
				this.chatSockets.delete(ws);
				sendToAll({
					type: 'chatSystem',
					text: `${username} left the chat`,
					ts: Date.now()
				});
				broadcastPresence();
			});
		}

		if (url.pathname === '/tracker') {
			this.trackerSockets.add(ws);

			ws.on('close', () => {
				this.trackerSockets.delete(ws);
			});

			const trackers = this.trackerSockets.size;
			const lastPeak = await kv.get('trackerPeak') || 0;

			if (trackers > lastPeak) {
				await kv.set('trackerPeak', trackers);
				if (this.env.USER_COUNT_WEBHOOK) {
					await fetch(this.env.USER_COUNT_WEBHOOK, {
						method: 'POST',
						headers: { 'Content-Type': 'application/json' },
						body: JSON.stringify({
							content: `new peak of ${trackers} people connected to the tracker socket`
						})
					});
				}
			}
		}
	}
}
