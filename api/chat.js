// api/chat.js
import { kv } from '@vercel/kv';

// CORS headers for all responses
const corsHeaders = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type',
    'Access-Control-Max-Age': '86400'
};

export default async function handler(req, res) {
    // Handle OPTIONS request for CORS preflight
    if (req.method === 'OPTIONS') {
        res.setHeader('Access-Control-Allow-Origin', '*');
        res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
        res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
        res.setHeader('Access-Control-Max-Age', '86400');
        return res.status(200).end();
    }

    const url = new URL(req.url, `http://${req.headers.host}`);
    const path = url.pathname.replace('/api/chat', '');

    // Set CORS headers for all responses
    Object.entries(corsHeaders).forEach(([key, value]) => {
        res.setHeader(key, value);
    });

    // Handle different chat endpoints
    if (path === '/send' && req.method === 'POST') {
        try {
            const body = JSON.parse(req.body || '{}');
            const { from, text } = body;
            
            if (!from || !text) {
                return res.status(400).json({ error: 'Missing from or text' });
            }

            const record = {
                type: 'chatMessage',
                from,
                text,
                ts: Date.now(),
                id: `${Date.now()}-${Math.random().toString(36).substring(2)}`
            };

            await kv.rpush('chat:history', JSON.stringify(record));
            await kv.ltrim('chat:history', -1000, -1);
            await kv.expire('chat:history', 60 * 60 * 24 * 7);

            return res.status(200).json({ ok: true, id: record.id, ts: record.ts });
        } catch (error) {
            return res.status(500).json({ error: error.message });
        }
    }

    if (path === '/messages' && req.method === 'GET') {
        try {
            const after = parseInt(url.searchParams.get('after') || '0');
            const history = await kv.lrange('chat:history', 0, -1) || [];
            
            const messages = history
                .map(msg => {
                    try {
                        return JSON.parse(msg);
                    } catch {
                        return null;
                    }
                })
                .filter(msg => msg && msg.ts > after)
                .sort((a, b) => a.ts - b.ts);

            return res.status(200).json({ messages });
        } catch (error) {
            return res.status(500).json({ error: error.message });
        }
    }

    if (path === '/presence' && req.method === 'POST') {
        try {
            const body = JSON.parse(req.body || '{}');
            const { from } = body;
            
            if (from) {
                await kv.set(`chat:presence:${from}`, JSON.stringify({ lastSeen: Date.now() }), { ex: 30 });
            }
            
            return res.status(200).json({ ok: true });
        } catch (error) {
            return res.status(500).json({ error: error.message });
        }
    }

    if (path === '/presence-count' && req.method === 'GET') {
        try {
            const keys = await kv.keys('chat:presence:*');
            return res.status(200).json({ count: keys?.length || 0 });
        } catch (error) {
            return res.status(500).json({ error: error.message });
        }
    }

    return res.status(404).json({ error: 'Not found' });
}