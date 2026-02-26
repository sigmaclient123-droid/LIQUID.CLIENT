// api/durable/websocket.js
import { kv } from '@vercel/kv';

export class WebSocketDurable {
	hamburburSockets = new Map();
	trackerSockets = new Set();

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
