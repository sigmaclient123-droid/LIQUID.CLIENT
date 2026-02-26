import { kv } from '@vercel/kv';
import fs from 'fs';
import path from 'path';
import { WebSocketDurable } from './durable/websocket.js';
import { handleDataManagement } from './data/data-manager.js';
import { uploadTrackingData, uploadS3RoomData } from './tracker/tracker-manager.js';

const liquidWS = new WebSocketDurable();

export default async function handler(req, res) {
	const url = new URL(req.url, `http://${req.headers.host}`);

	if (req.headers.upgrade === 'websocket') {
		return liquidWS.handleUpgrade(req.socket, req);
	}

	if (url.pathname.startsWith('/tracker')) {
		const action = url.pathname.replace('/tracker', '');
		switch (action) {
			case '/upload':
				return await uploadTrackingData(req, res);

			case '/upload/s3-room':
				return await uploadS3RoomData(req, res);

			default:
				return liquidWS.handleUpgrade(req.socket, req);
		}
	}

	if (url.pathname === '/dashboard' || url.pathname === '/dash') {
		const users = liquidWS.getUsers();
		return res.status(200).json({
			trackers: liquidWS.trackerSockets.size,
			liquids: users.map(u => u.username)
		});
	}

	if (['/data', '/json', '/serverdata'].includes(url.pathname) || url.pathname.startsWith('/liquiddata')) {
		let currentData = await kv.get('liquid_data');
		
		if (!currentData) {
			const fallbackPath = path.join(process.cwd(), 'api', 'data', 'data.json');
			if (fs.existsSync(fallbackPath)) {
				currentData = JSON.parse(fs.readFileSync(fallbackPath, 'utf8'));
			}
		}

		return res.status(200).json(currentData || { error: 'No data available' });
	}

	if (url.pathname === '/manage') {
		return await handleDataManagement(req, res);
	}

	if (url.pathname === '/' || url.pathname === '/liquid') {
		const htmlPath = path.join(process.cwd(), 'public', 'main-page.html');
		if (fs.existsSync(htmlPath)) {
			const html = fs.readFileSync(htmlPath, 'utf8');
			res.setHeader('Content-Type', 'text/html; charset=utf-8');
			return res.status(200).send(html);
		}
	}

	return res.status(404).json({
		status: 404,
		error: 'NotFound',
		message: "The URL you're looking for does not exist. Some common URLs here at liquid-theta.vercel.app are '/data' and '/dashboard'. Were you perhaps looking for those?"
	});
}