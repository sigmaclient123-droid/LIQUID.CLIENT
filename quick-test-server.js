// quick-test-server.js
// Lightweight HTTP server to run the Liquid API handler without Vercel.
// Use this when vercel dev is unstable on Windows (UV_HANDLE_CLOSING errors).

import http from 'http';
import dotenv from 'dotenv';
import handler from './api/index.js';

dotenv.config({ path: '.env.local' });

const port = process.env.PORT || 3000;

function createVercelResponse(res) {
    return {
        setHeader: res.setHeader.bind(res),
        status(code) {
            res.statusCode = code;
            return {
                json(obj) {
                    res.setHeader('Content-Type', 'application/json');
                    res.end(JSON.stringify(obj));
                },
                send(text) {
                    res.end(text);
                }
            };
        }
    };
}

const server = http.createServer((req, res) => {
    const vRes = createVercelResponse(res);
    handler(req, vRes).catch(err => {
        console.error('Handler threw error:', err);
        if (!res.headersSent) {
            res.statusCode = 500;
            res.end('Internal Server Error');
        }
    });
});

server.listen(port, () => {
    console.log(`🔧 Quick test server listening on http://localhost:${port}`);
    if (!process.env.DISCORD_WEBHOOK_URL) {
        console.warn('⚠️ DISCORD_WEBHOOK_URL is not set. Update .env.local with a valid webhook URL');
    } else {
        console.log('✅ Discord webhook URL is configured (will not verify its validity here)');
    }
});

server.on('error', (err) => {
    console.error('Server error:', err.message);
});
