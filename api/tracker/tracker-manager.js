import { kv } from '@vercel/kv';

export async function uploadS3RoomData(req, res) {
	const check = performRequestChecks(req);
	if (check.shouldReturn) return res.status(check.status).json(check.data);

	const roomData = req.body;
	let liquidData = await kv.get('liquid_data');

	if (!liquidData) {
		return res.status(500).json({ error: 'Data not found' });
	}

	for (const playerData of roomData.Players) {
		let actualPlayerName = liquidData.knownPeople[playerData.PlayerId];
		const cosmeticKeys = Object.keys(liquidData.specialCosmetics);
		let knownCosmetics = cosmeticKeys
			.filter(key => playerData.ConcatString.includes(key))
			.map(key => liquidData.specialCosmetics[key])
			.join(', ');

		if (!actualPlayerName && !knownCosmetics) {
			continue;
		}

		await handleTrackedPlayer({
			isUserKnown: !!actualPlayerName,
			username: actualPlayerName || 'Unknown',
			hasSpecialCosmetic: !!knownCosmetics,
			specialCosmetic: knownCosmetics,
			roomCode: roomData.RoomName,
			playersInRoom: roomData.Players.length,
			inGameName: playerData.Name,
			gameModeString: roomData.GameMode,
			userId: playerData.PlayerId
		});
	}
	
	return res.status(200).json({ success: true });
}

export async function uploadTrackingData(req, res) {
	const check = performRequestChecks(req);
	if (check.shouldReturn) return res.status(check.status).json(check.data);

	await handleTrackedPlayer(req.body);
	return res.status(200).json({ success: true });
}

function performRequestChecks(req) {
	if (req.method !== 'POST') {
		return {
			shouldReturn: true,
			status: 405,
			data: { error: 'MethodNotAllowed' }
		};
	}

	const authKey = req.headers['auth-key'];
	if (!authKey || authKey !== process.env.TRACKER_UPLOAD_SECRET_KEY) {
		return {
			shouldReturn: true,
			status: 401,
			data: { error: 'Unauthorized' }
		};
	}

	return { shouldReturn: false };
}

async function handleTrackedPlayer(trackingData) {
	const baseEmbed = {
		title: `Found ${trackingData.isUserKnown ? trackingData.username : 'someone'}${trackingData.hasSpecialCosmetic ? ` with ${trackingData.specialCosmetic}` : ''}!`,
		fields: [
			{ name: 'Room Code', value: String(trackingData.roomCode || 'N/A') },
			{ name: 'Players In Code', value: String(trackingData.playersInRoom || 'N/A') },
			{ name: 'In Game Name', value: String(trackingData.inGameName || 'N/A') },
			{ name: 'GameMode String', value: String(trackingData.gameModeString || 'N/A') },
			{ name: 'UserID', value: String(trackingData.userId || 'N/A') }
		],
		color: 0x2B265B
	};

	const sendWebhook = async (url, body) => {
		if (!url) return;
		try {
			await fetch(url, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(body)
			});
		} catch (e) {
			console.error('Webhook error:', e);
		}
	};

	await Promise.all([
		sendWebhook(process.env.GC_WEBHOOK, { embeds: [baseEmbed] }),
		sendWebhook(process.env.HDM_WEBHOOK, { content: '<@&1469410214876020786>', embeds: [baseEmbed] }),
		sendWebhook(process.env.MB_WEBHOOK, {
			username: 'Liquid Tracker',
			avatar_url: 'https://files.hamburbur.org/HamburburSuperAdmin.png',
			content: '<@&1474125765758029825>',
			embeds: [baseEmbed]
		})
	]);
}