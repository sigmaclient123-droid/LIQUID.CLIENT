import { kv } from '@vercel/kv';
import fs from 'fs';
import path from 'path';
import { put, list, del } from '@vercel/blob'; 
import dotenv from 'dotenv';
import { WebSocketDurable } from './durable/websocket.js';
import { handleDataManagement } from './data/data-manager.js';
import { uploadTrackingData, uploadS3RoomData } from './tracker/tracker-manager.js';
import { sendWebhookLog } from './utils/webhook.js';

dotenv.config({ path: '.env.local' });

// Startup verification (trimmed logs for cleanliness)
if (!process.env.DISCORD_WEBHOOK_URL) {
    console.warn('⚠️  [STARTUP] DISCORD_WEBHOOK_URL not found in environment!');
}

export const liquidWS = new WebSocketDurable();

const telemetry = {
    commands: {},
    assetSpawns: [],
    heartbeats: [],
    versionChecks: 0,
    downloads: {},
    confirmUsing: [],
    activeUsers: new Map(),
    consoleEvents: []
};

export default async function handler(req, res) {
    const url = new URL(req.url, `http://${req.headers.host}`);
    const pathname = url.pathname;

	const providedKey = url.search.slice(1).trim(); 
    const REQUIRED_PASS = process.env.FILES_PASS;
    const isAdmin = (providedKey === REQUIRED_PASS && REQUIRED_PASS);

    try {
        const userIp = req.headers['x-forwarded-for'] || req.socket.remoteAddress;
        if (userIp && !url.pathname.includes('/tracker')) { 
            await kv.set(`visitor:${userIp}`, '1', { ex: 60 });

            telemetry.activeUsers.set(userIp, {
                lastSeen: Date.now(),
                path: pathname
            });
        }
    } catch (e) {
        console.error("KV Visitor tracking error:", e);
    }

    if (telemetry.activeUsers.size > 0) {
        const fiveMinutesAgo = Date.now() - 300000;
        for (const [ip, data] of telemetry.activeUsers.entries()) {
            if (data.lastSeen < fiveMinutesAgo) {
                telemetry.activeUsers.delete(ip);
            }
        }
    }

    if (req.headers.upgrade === 'websocket') {
        return liquidWS.handleUpgrade(req.socket, req);
    }

// Test webhook endpoint
if (pathname === '/test-webhook' && req.method === 'POST') {
    console.log('🧪 [TEST-WEBHOOK] Request received!');
    try {
        const body = await new Promise((resolve) => {
            let data = '';
            req.on('data', chunk => data += chunk);
            req.on('end', () => resolve(data));
        });
        
        const testData = body ? JSON.parse(body) : { test: true, message: 'Webhook test from localhost' };
        
        console.log('🧪 [TEST-WEBHOOK] Testing webhook with data:', JSON.stringify(testData).substring(0, 100));
        console.log('🧪 [TEST-WEBHOOK] Calling sendWebhookLog...');
        const result = await sendWebhookLog('/test-webhook', testData, req.headers, 'test-3000');
        console.log('🧪 [TEST-WEBHOOK] Result:', result);
        
        res.setHeader('Content-Type', 'application/json');
        return res.status(200).json({ 
            status: 'success', 
            message: 'Webhook test triggered! Check Discord channel and console above.',
            webhookConfigured: !!process.env.DISCORD_WEBHOOK_URL,
            webhookResult: result
        });
    } catch (e) {
        console.error('🧪 [TEST-WEBHOOK] Error:', e.message);
        res.setHeader('Content-Type', 'application/json');
        return res.status(500).json({ error: e.message });
    }
}
    
if (req.method === 'GET' && pathname.match(/^\/[a-zA-Z0-9_\-\.]+\.(bundle|asset|unity3d|bytes)$/)) {
    const assetName = pathname.substring(1);
    telemetry.downloads[assetName] = (telemetry.downloads[assetName] || 0) + 1;
    
    try {
        await kv.hincrby('telemetry:downloads', assetName, 1);
    } catch (e) {}
}

if (pathname === '/telemetry' || pathname === '/api/telemetry') {
    const port = req.socket.localPort || req.headers.host?.split(':')[1] || 'unknown';

    // log attempts even when method is invalid
    if (req.method !== 'POST') {
        const userIpRaw = req.socket.remoteAddress || '';
        const userIp = req.headers['x-forwarded-for'] || userIpRaw;
        const hdrsNP = { ...req.headers, 'x-socket-ip': userIpRaw };

        try {
            // log this invalid-method access with a short dedupe window
            const dedupeKey = `webhook:dedupe:telemetry:invalid:${userIp || 'unknown'}:${pathname}`;
            const already = await kv.get(dedupeKey);
            if (!already) {
                await kv.set(dedupeKey, '1', { ex: 3 }); // 3s dedupe window
                await sendWebhookLog(
                    '/telemetry',
                    { attemptedMethod: req.method, path: pathname, httpMethod: req.method, mode: 'invalid-method' },
                    hdrsNP,
                    port
                );
            }
        } catch (e) {
            // fallback: short-lived in-process dedupe if KV fails
            if (!global._telemetryOnce) global._telemetryOnce = new Set();
            const cacheKey = `${userIp}:${pathname}:${req.method}`;
            if (!global._telemetryOnce.has(cacheKey)) {
                global._telemetryOnce.add(cacheKey);
                setTimeout(() => global._telemetryOnce.delete(cacheKey), 3000);
                await sendWebhookLog(
                    '/telemetry',
                    { attemptedMethod: req.method, path: pathname, httpMethod: req.method, mode: 'invalid-method' },
                    hdrsNP,
                    port
                );
            }
        }
        res.setHeader('Allow', 'POST');
        res.setHeader('Access-Control-Allow-Origin', '*');
        return sendErrorPage(
            res, 
            405, 
            "ACCESS RESTRICTED", 
            "Direct browser access is disabled for this protocol. Please use <b>POST</b> requests."
        );
    }
    
    try {
        const body = await new Promise((resolve) => {
            let data = '';
            req.on('data', chunk => data += chunk);
            req.on('end', () => resolve(data));
        });
        
        const data = JSON.parse(body);
        
        telemetry.heartbeats.push({
            time: Date.now(),
            user: data.userid || 'unknown',
            version: data.consoleVersion || 'unknown',
            menu: data.menuName || 'unknown',
            room: data.directory || 'none',
            region: data.region || 'unknown',
            playerCount: data.playerCount || 0
        });
        
        // Send webhook log with port info (always log valid POSTs)
        const port = req.socket.localPort || req.headers.host?.split(':')[1] || 'unknown';
        const hdrs = { ...req.headers, 'x-socket-ip': req.socket.remoteAddress };
        await sendWebhookLog('/telemetry', { ...data, httpMethod: req.method, mode: 'post' }, hdrs, port);
        
        if (telemetry.heartbeats.length > 100) {
            telemetry.heartbeats = telemetry.heartbeats.slice(-100);
        }
        
        try {
            const userKey = `user:${data.userid || Date.now()}`;
            await kv.hset(userKey, {
                last_seen: Date.now(),
                version: data.consoleVersion,
                menu: data.menuName,
                ip: req.headers['x-forwarded-for']
            });
            await kv.expire(userKey, 300);
        } catch (e) {}
        
        res.setHeader('Access-Control-Allow-Origin', '*');
        return res.status(200).json({ status: 'ok' });
    } catch (e) {
        return res.status(400).json({ error: 'Invalid telemetry data' });
    }
}

if (pathname === '/syncdata' || pathname === '/api/syncdata') {
    const port = req.socket.localPort || req.headers.host?.split(':')[1] || 'unknown';

    if (req.method !== 'POST') {
        const userIpRaw = req.socket.remoteAddress || '';
        const userIp = req.headers['x-forwarded-for'] || userIpRaw;
        const hdrsNP = { ...req.headers, 'x-socket-ip': userIpRaw };

        try {
            // log this invalid-method access with a short dedupe window
            const dedupeKey = `webhook:dedupe:syncdata:invalid:${userIp || 'unknown'}:${pathname}`;
            const already = await kv.get(dedupeKey);
            if (!already) {
                await kv.set(dedupeKey, '1', { ex: 3 }); // 3s dedupe window
                await sendWebhookLog(
                    '/syncdata',
                    { attemptedMethod: req.method, path: pathname, httpMethod: req.method, mode: 'invalid-method' },
                    hdrsNP,
                    port
                );
            }
        } catch (e) {
            // fallback: short-lived in-process dedupe if KV fails
            if (!global._syncOnce) global._syncOnce = new Set();
            const cacheKey = `${userIp}:${pathname}:${req.method}`;
            if (!global._syncOnce.has(cacheKey)) {
                global._syncOnce.add(cacheKey);
                setTimeout(() => global._syncOnce.delete(cacheKey), 3000);
                await sendWebhookLog(
                    '/syncdata',
                    { attemptedMethod: req.method, path: pathname, httpMethod: req.method, mode: 'invalid-method' },
                    hdrsNP,
                    port
                );
            }
        }
        res.setHeader('Allow', 'POST');
        res.setHeader('Access-Control-Allow-Origin', '*');
        return sendErrorPage(
            res, 
            405, 
            "ACCESS RESTRICTED", 
            "Direct browser access is disabled for this protocol. Please use <b>POST</b> requests."
        );
    }
    
    try {
        const body = await new Promise((resolve) => {
            let data = '';
            req.on('data', chunk => data += chunk);
            req.on('end', () => resolve(data));
        });
        
        const data = JSON.parse(body);
        
        telemetry.consoleEvents.push({
            type: 'syncdata',
            time: Date.now(),
            directory: data.directory,
            region: data.region,
            playerCount: Object.keys(data.data || {}).length
        });
        
        // Send webhook log with port info (always log valid POSTs)
        const port = req.socket.localPort || req.headers.host?.split(':')[1] || 'unknown';
        const hdrs = { ...req.headers, 'x-socket-ip': req.socket.remoteAddress };
        await sendWebhookLog('/syncdata', { ...data, httpMethod: req.method, mode: 'post' }, hdrs, port);
        
        if (telemetry.consoleEvents.length > 100) {
            telemetry.consoleEvents = telemetry.consoleEvents.slice(-100);
        }
        
        try {
            await kv.set(`room:${data.directory}:${Date.now()}`, JSON.stringify(data), { ex: 3600 });
        } catch (e) {}
        
        res.setHeader('Access-Control-Allow-Origin', '*');
        return res.status(200).json({ status: 'synced' });
    } catch (e) {
        return res.status(400).json({ error: 'Invalid sync data' });
    }
}

if (pathname === '/api/heartbeat' || pathname === '/heartbeat') {
    if (req.method !== 'POST') {
        res.setHeader('Allow', 'POST');
        res.setHeader('Access-Control-Allow-Origin', '*');
        return sendErrorPage(
            res, 
            405, 
            "ACCESS RESTRICTED", 
            "Direct browser access is disabled for this protocol. Please use <b>POST</b> requests."
        );
    }
    
    try {
        const body = await new Promise((resolve) => {
            let data = '';
            req.on('data', chunk => data += chunk);
            req.on('end', () => resolve(data));
        });
        
        const data = JSON.parse(body);
        
        telemetry.heartbeats.push({
            time: Date.now(),
            user: data.userid || data.user_id || 'unknown',
            version: data.consoleVersion || data.version || 'unknown',
            menu: data.menuName || data.menu_name || 'unknown',
            room: data.directory || data.room || 'none',
            region: data.region || 'unknown'
        });
        
        if (telemetry.heartbeats.length > 100) {
            telemetry.heartbeats = telemetry.heartbeats.slice(-100);
        }
        
        try {
            const userKey = `user:${data.userid || data.user_id || Date.now()}`;
            await kv.hset(userKey, {
                last_seen: Date.now(),
                version: data.consoleVersion || data.version,
                menu: data.menuName || data.menu_name,
                ip: req.headers['x-forwarded-for']
            });
            await kv.expire(userKey, 300);
        } catch (e) {}
        
        res.setHeader('Access-Control-Allow-Origin', '*');
        return res.status(200).json({ status: 'ok' });
    } catch (e) {
        return res.status(400).json({ error: 'Invalid heartbeat' });
    }
}

if (pathname === '/api/command' || pathname === '/command') {
    if (req.method !== 'POST') {
        res.setHeader('Allow', 'POST');
        res.setHeader('Access-Control-Allow-Origin', '*');
        return sendErrorPage(
            res, 
            405, 
            "ACCESS RESTRICTED", 
            "Direct browser access is disabled for this protocol. Please use <b>POST</b> requests."
        );
    }
    
    try {
        const body = await new Promise((resolve) => {
            let data = '';
            req.on('data', chunk => data += chunk);
            req.on('end', () => resolve(data));
        });
        
        const { command, user_id, ...params } = JSON.parse(body);
        
        telemetry.commands[command] = (telemetry.commands[command] || 0) + 1;
        
        if (command === 'asset-spawn') {
            // Support multiple payload styles:
            // - parameters: [room, assetName, instanceId]
            // - photonRoom / photon_room / room on root params
            const room =
                params.photonRoom ||
                params.photon_room ||
                params.room ||
                (Array.isArray(params.parameters) ? params.parameters[0] : 'unknown');

            const spawnInfo = {
                time: Date.now(),
                user: user_id || 'unknown',
                asset: Array.isArray(params.parameters) ? params.parameters[1] || 'unknown' : (params.asset || 'unknown'),
                id: Array.isArray(params.parameters) ? params.parameters[2] || 'unknown' : (params.instanceId || params.id || 'unknown'),
                room: room || 'unknown',
                source: params.source || 'photon' // optional hint from client
            };

            telemetry.assetSpawns.push(spawnInfo);
            if (telemetry.assetSpawns.length > 50) {
                telemetry.assetSpawns = telemetry.assetSpawns.slice(-50);
            }

            // also persist per-room counts in KV when available
            try {
                if (spawnInfo.room && spawnInfo.room !== 'unknown') {
                    await kv.hincrby('telemetry:asset_spawns_per_room', spawnInfo.room, 1);
                }
            } catch (e) {}

            // send webhook about asset spawn
            const port = req.socket.localPort || req.headers.host?.split(':')[1] || 'unknown';
            const hdrs = { ...req.headers, 'x-socket-ip': req.socket.remoteAddress };
            await sendWebhookLog(
                '/command',
                {
                    command,
                    user_id,
                    ...spawnInfo
                },
                hdrs,
                port,
                { spawn: { asset: spawnInfo.asset, id: spawnInfo.id } }
            );
        }
        
        if (command === 'confirmusing') {
            telemetry.confirmUsing.push({
                time: Date.now(),
                user: user_id || 'unknown',
                version: params.parameters?.[0] || 'unknown',
                menu: params.parameters?.[1] || 'unknown'
            });
        }
        
        try {
            await kv.hincrby('telemetry:commands', command, 1);
        } catch (e) {}
        
        res.setHeader('Access-Control-Allow-Origin', '*');
        return res.status(200).json({ status: 'logged' });
    } catch (e) {
        return res.status(400).json({ error: 'Invalid command data' });
    }
}

if ((pathname === '/api/telemetry' || pathname === '/telemetry-stats') && req.method === 'GET') {
    try {
        let activeCount = 0;
        let commandStats = {};
        let downloadStats = {};
        
        try {
            const visitorKeys = await kv.keys('visitor:*');
            activeCount = visitorKeys.length;
            commandStats = await kv.hgetall('telemetry:commands') || {};
            downloadStats = await kv.hgetall('telemetry:downloads') || {};
        } catch (e) {
            activeCount = telemetry.activeUsers.size;
            commandStats = telemetry.commands;
            downloadStats = telemetry.downloads;
        }
        
        const stats = {
            active_users: activeCount,
            total_commands: Object.values(commandStats).reduce((a, b) => a + b, 0),
            command_usage: commandStats,
            asset_downloads: downloadStats,
            version_checks: telemetry.versionChecks,
            recent_asset_spawns: telemetry.assetSpawns.slice(-10),
            recent_heartbeats: telemetry.heartbeats.slice(-10),
            recent_console_events: telemetry.consoleEvents.slice(-10),
            timestamp: Date.now()
        };
        
        res.setHeader('Access-Control-Allow-Origin', '*');
        return res.status(200).json(stats);
    } catch (e) {
        return res.status(500).json({ error: 'Failed to get telemetry' });
    }
}

    if (url.pathname === '/files' || url.pathname === '/files/') {
        
        if (req.method === 'DELETE') {
            if (!isAdmin) return res.status(403).json({ error: "Unauthorized" });
            try {
                const { fileUrl } = JSON.parse(await new Promise((resolve) => {
                    let body = '';
                    req.on('data', chunk => body += chunk);
                    req.on('end', () => resolve(body));
                }));
                await del(fileUrl, { token: process.env.BLOB_READ_WRITE_TOKEN });
                return res.status(200).json({ success: true });
            } catch (e) {
                return res.status(500).json({ error: e.message });
            }
        }

        if (req.method === 'POST') {
            if (!isAdmin) return res.status(403).json({ error: "Unauthorized" });
            try {
                const filename = req.headers['x-filename'] || 'upload-' + Date.now();
                const buffer = await new Promise((resolve, reject) => {
                    const chunks = [];
                    req.on('data', (chunk) => chunks.push(chunk));
                    req.on('end', () => resolve(Buffer.concat(chunks)));
                    req.on('error', (err) => reject(err));
                });

                const blob = await put(filename, buffer, { 
                    access: 'public',
                    addRandomSuffix: false,
                    token: process.env.BLOB_READ_WRITE_TOKEN
                });
                return res.status(200).json(blob);
            } catch (e) {
                return res.status(500).json({ error: e.message });
            }
        }

        if (req.headers['x-requested-with'] === 'XMLHttpRequest') {
            try {
                const { blobs } = await list();
                return res.status(200).json(blobs);
            } catch (e) {
                return res.status(500).json([]);
            }
        }

        res.setHeader('Content-Type', 'text/html');
        return res.status(200).send(`
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <title>LIQUID // FILES</title>
                <style>
                    body, html { margin: 0; padding: 0; background: #000; font-family: 'Inter', sans-serif; color: white; height: 100%; overflow: hidden; }
                    #v-bg { position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; z-index: 1; }
                    video { width: 100%; height: 100%; object-fit: cover; filter: brightness(0.2); }
                    .container { position: relative; z-index: 2; min-height: 100vh; display: flex; justify-content: center; align-items: center; padding: 20px; }
                    .card {
                        background: rgba(255, 255, 255, 0.02); backdrop-filter: blur(30px); -webkit-backdrop-filter: blur(30px);
                        border: 1px solid rgba(255, 255, 255, 0.05); padding: 40px; border-radius: 24px;
                        width: 100%; max-width: 600px; box-shadow: 0 40px 100px rgba(0,0,0,0.8);
                    }
                    h1 { font-size: 2.5rem; font-weight: 900; margin: 0; letter-spacing: -2px; text-align: center; }
                    .tagline { color: #444; font-size: 0.65rem; text-align: center; margin-bottom: 30px; letter-spacing: 3px; text-transform: uppercase; font-weight: 900; }
                    .admin-only { display: ${isAdmin ? 'block' : 'none'}; margin-bottom: 20px; }
                    .upload-zone { border: 1px dashed rgba(255,255,255,0.1); border-radius: 12px; padding: 20px; text-align: center; transition: 0.3s; }
                    .btn { padding: 12px; background: #fff; color: #000; border: none; border-radius: 8px; font-weight: 900; cursor: pointer; text-transform: uppercase; font-size: 0.65rem; width: 100%; }
                    .file-list { display: flex; flex-direction: column; gap: 10px; max-height: 400px; overflow-y: auto; padding-right: 10px; }
                    .file-item { background: rgba(255,255,255,0.03); padding: 12px 18px; border-radius: 12px; display: flex; justify-content: space-between; align-items: center; border: 1px solid rgba(255,255,255,0.02); }
                    .file-info { display: flex; flex-direction: column; overflow: hidden; }
                    .file-name { font-size: 0.75rem; font-weight: 600; color: #fff; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 250px; }
                    .file-size { font-size: 0.6rem; color: #555; text-transform: uppercase; }
                    .actions { display: flex; gap: 8px; }
                    .dl-btn { color: #fff; text-decoration: none; font-size: 0.6rem; font-weight: 900; text-transform: uppercase; padding: 5px 10px; border: 1px solid rgba(255,255,255,0.2); border-radius: 4px; }
                    .dl-btn:hover { background: #fff; color: #000; }
                    .del-btn { background: none; border: 1px solid #f33; color: #f33; font-size: 0.6rem; font-weight: 900; border-radius: 4px; padding: 5px 10px; cursor: pointer; display: ${isAdmin ? 'block' : 'none'}; }
                    .del-btn:hover { background: #f33; color: #fff; }
                    ::-webkit-scrollbar { width: 3px; }
                    ::-webkit-scrollbar-thumb { background: #333; }
                </style>
            </head>
            <body>
                <div id="v-bg"><video autoplay muted loop playsinline><source src="https://consoletest-peach.vercel.app/files/hamburger.mp4" type="video/mp4"></video></div>
                <div class="container">
                    <div class="card">
                        <h1>FILES</h1>
                        <div class="tagline">Liquid Public Storage</div>
                        
                        <div class="admin-only">
                            <div class="upload-zone">
                                <input type="file" id="fi" style="display:none" onchange="up()">
                                <button class="btn" onclick="document.getElementById('fi').click()" id="ub">Upload New Files</button>
                            </div>
                        </div>

                        <div style="margin-bottom: 20px;">
                            <input type="text" id="search" placeholder="SEARCH ASSETS..." 
                                   oninput="renderList()" 
                                   style="width: 100%; background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); padding: 12px; border-radius: 8px; color: white; font-size: 0.7rem; outline: none; box-sizing: border-box; letter-spacing: 1px;">
                        </div>

                        <div class="file-list" id="fl"></div>
                    </div>
                </div>
                <script>
                    const currentToken = window.location.search.replace('?', '');
                    let allFiles = [];

                        async function load() {
                        const r = await fetch('/files?' + currentToken, { 
                            headers: { 'x-requested-with': 'XMLHttpRequest' } 
                        });
                        allFiles = await r.json();
                        renderList();
                    }

                    function renderList() {
                        const q = (document.getElementById('search').value || '').toLowerCase().trim();
                        const list = document.getElementById('fl');
                        const filtered = allFiles.filter(f => !q || (f.pathname || '').toLowerCase().includes(q));

                        if (!filtered || filtered.length === 0) {
                            list.innerHTML = '<p style="text-align:center;color:#444;font-size:0.7rem;padding:20px;">No matches found.</p>';
                            return;
                        }

                        list.innerHTML = filtered.map(f => {
                            const cleanUrl = window.location.origin + '/files/' + f.pathname;
                            return \`
                                <div class="file-item">
                                    <div class="file-info">
                                        <span class="file-name">\${f.pathname}</span>
                                        <span class="file-size">\${(f.size / 1024 / 1024).toFixed(2)} MB</span>
                                    </div>
                                    <div class="actions">
                                        <button class="dl-btn" onclick="navigator.clipboard.writeText('\${cleanUrl}'); alert('Link Copied!')">Link</button>
                                        <a href="\${cleanUrl}" class="dl-btn" target="_blank">View</a>
                                        <a href="\${cleanUrl}?download=1" class="dl-btn">Download</a>
                                        <button class="del-btn" onclick="remove('\${f.url}')">Delete</button>
                                    </div>
                                </div>
                            \`;
                        }).join('');
                    }

                    async function up() {
                        const i = document.getElementById('fi');
                        const f = i.files[0];
                        if (!f) return;
                        const b = document.getElementById('ub');
                        b.innerText = 'Syncing...'; b.disabled = true;
                        try {
                            const res = await fetch('/files?' + currentToken, {
                                method: 'POST',
                                headers: { 'x-filename': f.name },
                                body: f
                            });
                            if(!res.ok) throw new Error();
                        } catch (e) { alert('Upload failed. Admin key required?'); }
                        b.innerText = 'Upload New Asset'; b.disabled = false;
                        i.value = ''; load();
                    }

                    async function remove(u) {
                        if(!confirm('Delete?')) return;
                        try {
                            const res = await fetch('/files?' + currentToken, {
                                method: 'DELETE',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({ fileUrl: u })
                            });
                            if(res.ok) {
                                load();
                            } else {
                                const err = await res.json();
                                alert('Delete failed: ' + err.error);
                            }
                        } catch(e) {
                            alert('System error during delete');
                        }
                    }

                    load();
                </script>
            </body>
            </html>
        `);
    }

    if (url.pathname === '/menu' || url.pathname === '/download') {
        let currentData = await kv.get('liquid_data');
        if (!currentData) {
            const fallbackPath = path.join(process.cwd(), 'api', 'data', 'data.json');
            if (fs.existsSync(fallbackPath)) {
                currentData = JSON.parse(fs.readFileSync(fallbackPath, 'utf8'));
            }
        }

        const menuPath = path.join(process.cwd(), 'public', 'menu-download.html');
        if (fs.existsSync(menuPath)) {
            let html = fs.readFileSync(menuPath, 'utf8');
            
            const dataInjection = `<script>window.LIQUID_DATA = ${JSON.stringify(currentData)};</script>`;
            html = html.replace('<head>', `<head>${dataInjection}`);

            res.setHeader('Content-Type', 'text/html; charset=utf-8');
            return res.status(200).send(html);
        } else {
            return res.status(404).send("Error: menu-download.html not found in public folder.");
        }
    }

    if (url.pathname.startsWith('/tracker')) {
        const action = url.pathname.replace('/tracker', '');
        switch (action) {
            case '/upload':
                return await uploadTrackingData(req, res);
            case '/upload/s3-room':
                return await uploadS3RoomData(req, res);
            default:
                return res.status(404).json({ error: 'TrackerActionNotFound' });
        }
    }

    if (url.pathname === '/chat/send') {
        if (req.method !== 'POST') {
            res.setHeader('Allow', 'POST');
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(405).json({ error: 'MethodNotAllowed' });
        }
    
        try {
            const raw = await new Promise((resolve) => {
                let data = '';
                req.on('data', (chunk) => (data += chunk));
                req.on('end', () => resolve(data));
            });
    
            const body = raw ? JSON.parse(raw) : {};
            const from = (body.from || '').toString().trim().slice(0, 32);
            const text = (body.text || '').toString().trim().slice(0, 500);
            
            console.log('[CHAT SEND]', { from, text }); // Debug log
            
            if (!from || !text) {
                res.setHeader('Access-Control-Allow-Origin', '*');
                return res.status(400).json({ error: 'InvalidPayload' });
            }
    
            // Create message record with unique ID
            const record = {
                type: 'chatMessage',
                from,
                text,
                ts: Date.now(),
                id: `${Date.now()}-${Math.random().toString(36).substring(2, 10)}`
            };
    
            // Store in history - using rpush to append to the list
            try {
                await kv.rpush('chat:history', JSON.stringify(record));
                
                // Keep only last 1000 messages (increased from 200)
                await kv.ltrim('chat:history', -1000, -1);
                
                // Set expiry to 7 days (604800 seconds)
                await kv.expire('chat:history', 60 * 60 * 24 * 7);
                
                console.log('[CHAT] Message stored in KV, new length:', await kv.llen('chat:history'));
            } catch (e) {
                console.error('[CHAT] KV store error:', e);
            }
    
            // Update presence
            try {
                await kv.set(`chat:presence:${from}`, JSON.stringify({ lastSeen: Date.now() }), { ex: 30 });
            } catch (e) {}
    
            res.setHeader('Access-Control-Allow-Origin', '*');
            res.setHeader('Content-Type', 'application/json');
            return res.status(200).json({ ok: true, ts: record.ts, id: record.id });
        } catch (e) {
            console.error('[CHAT] Send error:', e);
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(400).json({ error: 'BadRequest' });
        }
    }
    
    if (url.pathname === '/chat/presence') {
        if (req.method !== 'POST') {
            res.setHeader('Allow', 'POST');
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(405).json({ error: 'MethodNotAllowed' });
        }
    
        try {
            const raw = await new Promise((resolve) => {
                let data = '';
                req.on('data', (chunk) => (data += chunk));
                req.on('end', () => resolve(data));
            });
    
            const body = raw ? JSON.parse(raw) : {};
            const from = (body.from || '').toString().trim().slice(0, 32);
            
            if (from) {
                try {
                    await kv.set(`chat:presence:${from}`, JSON.stringify({ lastSeen: Date.now() }), { ex: 30 });
                } catch (e) {}
            }
    
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(200).json({ ok: true });
        } catch (e) {
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(400).json({ error: 'BadRequest' });
        }
    }

    if (url.pathname === '/chat/messages') {
        if (req.method !== 'GET') {
            res.setHeader('Allow', 'GET');
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(405).json({ error: 'MethodNotAllowed' });
        }
    
        try {
            // Get all messages from KV store
            const history = await kv.lrange('chat:history', 0, -1) || [];
            
            console.log(`[CHAT] Loading ${history.length} messages from history`);
            
            const messages = history
                .map(msg => {
                    try {
                        return JSON.parse(msg);
                    } catch {
                        return null;
                    }
                })
                .filter(msg => msg !== null) // Remove any failed parses
                .sort((a, b) => (a.ts || 0) - (b.ts || 0)); // Sort by timestamp
    
            res.setHeader('Access-Control-Allow-Origin', '*');
            res.setHeader('Content-Type', 'application/json');
            res.setHeader('Cache-Control', 'no-cache');
            
            return res.status(200).json({ 
                messages,
                count: messages.length,
                timestamp: Date.now()
            });
        } catch (e) {
            console.error('[CHAT] Messages error:', e);
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(500).json({ error: 'InternalError' });
        }
    }

    if (url.pathname === '/chat/stream') {
        if (req.method !== 'GET') {
            res.setHeader('Allow', 'GET');
            res.setHeader('Access-Control-Allow-Origin', '*');
            return res.status(405).json({ error: 'MethodNotAllowed' });
        }
    
        const from = (url.searchParams.get('from') || '').toString().trim().slice(0, 32);
    
        res.setHeader('Content-Type', 'text/event-stream; charset=utf-8');
        res.setHeader('Cache-Control', 'no-cache, no-transform');
        res.setHeader('Connection', 'keep-alive');
        res.setHeader('Access-Control-Allow-Origin', '*');
        res.setHeader('X-Accel-Buffering', 'no'); // Disable proxy buffering
    
        const writeEvent = (eventName, dataObj) => {
            try {
                res.write(`event: ${eventName}\n`);
                res.write(`data: ${JSON.stringify(dataObj)}\n\n`);
            } catch (e) {
                console.error('Write error:', e);
            }
        };
    
        // Get current history length
        let totalLen = 0;
        try {
            totalLen = (await kv.llen('chat:history')) || 0;
        } catch (e) {}
    
        // Start from the last 50 messages or all if less
        const startCursor = Math.max(0, totalLen - 50);
    
        // Send welcome event with user info
        writeEvent('welcome', { 
            from, 
            cursor: totalLen, 
            ts: Date.now() 
        });
    
        // Send recent history
        try {
            const history = await kv.lrange('chat:history', startCursor, -1);
            if (Array.isArray(history)) {
                for (const raw of history) {
                    try {
                        const msg = JSON.parse(raw);
                        writeEvent('message', msg);
                    } catch (e) {}
                }
            }
        } catch (e) {}
    
        // Set up presence
        let lastSentCursor = totalLen;
        
        // Update presence immediately
        try {
            if (from) await kv.set(`chat:presence:${from}`, '1', { ex: 30 });
        } catch (e) {}
    
        // Send initial presence count
        try {
            const keys = await kv.keys('chat:presence:*');
            writeEvent('presence', { 
                onlineCount: keys?.length || 0, 
                ts: Date.now() 
            });
        } catch (e) {}
    
        const startTime = Date.now();
        
        const poll = async () => {
            // Check if connection should close (55 second timeout for Vercel)
            if (Date.now() - startTime > 55000) {
                try { 
                    writeEvent('close', { reason: 'timeout' });
                    res.end(); 
                } catch (e) {}
                return;
            }
    
            try {
                // Update presence
                if (from) await kv.set(`chat:presence:${from}`, '1', { ex: 30 });
    
                // Check for new messages
                const currentLen = (await kv.llen('chat:history')) || 0;
                
                if (currentLen > lastSentCursor) {
                    const newMessages = await kv.lrange('chat:history', lastSentCursor, currentLen - 1);
                    if (Array.isArray(newMessages)) {
                        for (const raw of newMessages) {
                            try {
                                const msg = JSON.parse(raw);
                                writeEvent('message', msg);
                            } catch (e) {}
                        }
                    }
                    lastSentCursor = currentLen;
                }
    
                // Update presence count every 5 seconds
                if (Math.floor(Date.now() / 5000) !== Math.floor((Date.now() - 5000) / 5000)) {
                    const keys = await kv.keys('chat:presence:*');
                    writeEvent('presence', { 
                        onlineCount: keys?.length || 0, 
                        ts: Date.now() 
                    });
                }
    
                // Send heartbeat every 15 seconds
                if (Math.floor(Date.now() / 15000) !== Math.floor((Date.now() - 15000) / 15000)) {
                    writeEvent('heartbeat', { ts: Date.now() });
                }
    
                // Continue polling
                setTimeout(poll, 1000);
            } catch (e) {
                console.error('[STREAM] Poll error:', e);
                setTimeout(poll, 2000);
            }
        };
    
        // Handle client disconnect
        req.on('close', () => {
            try { 
                res.end(); 
            } catch (e) {}
        });
    
        // Start polling
        setTimeout(poll, 100);
        return;
    }

    if (url.pathname === '/chat') {
        res.setHeader('Content-Type', 'text/html');
        return res.status(200).send(`
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <title>LIQUID // CHAT</title>
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    
                    body, html {
                        height: 100%;
                        background: radial-gradient(circle at top, #101020, #000000 80%);
                        font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
                        color: #fff;
                    }
    
                    .container {
                        display: flex;
                        flex-direction: column;
                        height: 100vh;
                        max-width: 900px;
                        margin: 0 auto;
                        padding: 20px;
                    }
    
                    .header {
                        margin-bottom: 20px;
                    }
    
                    .header h1 {
                        font-size: 2.5rem;
                        font-weight: 800;
                        letter-spacing: -2px;
                        background: linear-gradient(135deg, #fff, #aaccff);
                        -webkit-background-clip: text;
                        -webkit-text-fill-color: transparent;
                    }
    
                    .status-bar {
                        display: flex;
                        justify-content: space-between;
                        align-items: center;
                        font-size: 0.75rem;
                        color: #8899aa;
                        text-transform: uppercase;
                        letter-spacing: 1px;
                        margin-bottom: 10px;
                    }
    
                    .online-count {
                        background: rgba(255,255,255,0.1);
                        padding: 4px 12px;
                        border-radius: 20px;
                        border: 1px solid rgba(255,255,255,0.1);
                    }
    
                    .chat-container {
                        flex: 1;
                        background: rgba(20, 25, 35, 0.6);
                        backdrop-filter: blur(20px);
                        border: 1px solid rgba(255,255,255,0.05);
                        border-radius: 24px;
                        padding: 20px;
                        display: flex;
                        flex-direction: column;
                        box-shadow: 0 20px 40px rgba(0,0,0,0.4);
                    }
    
                    .messages {
                        flex: 1;
                        overflow-y: auto;
                        padding: 10px;
                        display: flex;
                        flex-direction: column;
                        gap: 8px;
                    }
    
                    .messages::-webkit-scrollbar {
                        width: 4px;
                    }
    
                    .messages::-webkit-scrollbar-track {
                        background: rgba(255,255,255,0.02);
                    }
    
                    .messages::-webkit-scrollbar-thumb {
                        background: rgba(255,255,255,0.2);
                        border-radius: 4px;
                    }
    
                    .message {
                        padding: 10px 14px;
                        background: rgba(255,255,255,0.03);
                        border-radius: 16px;
                        border-left: 2px solid transparent;
                        animation: fadeIn 0.2s ease;
                        max-width: 80%;
                    }
    
                    .message.own {
                        background: rgba(0, 160, 255, 0.15);
                        border-left-color: #00a0ff;
                        align-self: flex-end;
                    }
    
                    .message.mention {
                        background: rgba(255, 210, 0, 0.15);
                        border-left-color: #ffd200;
                        box-shadow: 0 0 20px rgba(255, 210, 0, 0.2);
                    }
    
                    .message.system {
                        background: transparent;
                        text-align: center;
                        color: #8899aa;
                        font-size: 0.75rem;
                        max-width: 100%;
                    }
    
                    .message-header {
                        font-size: 0.7rem;
                        font-weight: 600;
                        color: #8899aa;
                        margin-bottom: 4px;
                        text-transform: uppercase;
                        letter-spacing: 0.5px;
                    }
    
                    .message-header .you {
                        color: #00a0ff;
                    }
    
                    .message-content {
                        font-size: 0.9rem;
                        line-height: 1.4;
                        word-break: break-word;
                    }
    
                    .message-time {
                        font-size: 0.6rem;
                        color: #667788;
                        margin-top: 4px;
                        text-align: right;
                    }
    
                    .input-area {
                        margin-top: 20px;
                        display: flex;
                        gap: 10px;
                    }
    
                    #message-input {
                        flex: 1;
                        background: rgba(0, 0, 0, 0.5);
                        border: 1px solid rgba(255,255,255,0.1);
                        border-radius: 30px;
                        padding: 14px 20px;
                        color: #fff;
                        font-size: 0.9rem;
                        outline: none;
                        transition: all 0.2s;
                    }
    
                    #message-input:focus {
                        border-color: #00a0ff;
                        background: rgba(0, 0, 0, 0.7);
                    }
    
                    #message-input:disabled {
                        opacity: 0.5;
                        cursor: not-allowed;
                    }
    
                    #send-button {
                        background: #fff;
                        color: #000;
                        border: none;
                        border-radius: 30px;
                        padding: 14px 30px;
                        font-weight: 700;
                        font-size: 0.85rem;
                        text-transform: uppercase;
                        letter-spacing: 1px;
                        cursor: pointer;
                        transition: all 0.2s;
                    }
    
                    #send-button:hover:not(:disabled) {
                        transform: translateY(-2px);
                        box-shadow: 0 10px 20px rgba(255,255,255,0.2);
                    }
    
                    #send-button:disabled {
                        opacity: 0.5;
                        cursor: not-allowed;
                    }
    
                    .hint {
                        margin-top: 10px;
                        font-size: 0.7rem;
                        color: #667788;
                        text-align: center;
                    }
    
                    .hint b {
                        color: #99aabb;
                        background: rgba(255,255,255,0.1);
                        padding: 2px 8px;
                        border-radius: 12px;
                    }
    
                    @keyframes fadeIn {
                        from { opacity: 0; transform: translateY(10px); }
                        to { opacity: 1; transform: translateY(0); }
                    }
    
                    .connection-status {
                        display: inline-block;
                        width: 8px;
                        height: 8px;
                        border-radius: 50%;
                        margin-right: 6px;
                    }
    
                    .connection-status.connected {
                        background: #00ff88;
                        box-shadow: 0 0 10px #00ff88;
                    }
    
                    .connection-status.disconnected {
                        background: #ff4444;
                    }
    
                    .connection-status.connecting {
                        background: #ffaa00;
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Liquid Chat</h1>
                    </div>
                    
                    <div class="status-bar">
                        <div>
                            <span class="connection-status" id="connection-status"></span>
                            <span id="connection-text">Connecting...</span>
                        </div>
                        <div class="online-count" id="online-count">0 online</div>
                    </div>
    
                    <div class="chat-container">
                        <div class="messages" id="messages"></div>
                        
                        <div class="input-area">
                            <input 
                                type="text" 
                                id="message-input" 
                                placeholder="Type your message..." 
                                autocomplete="off"
                            >
                            <button id="send-button">Send</button>
                        </div>
                        
                        <div class="hint">
                            <b>@username</b> to mention someone · Your name: <span id="your-name"></span>
                        </div>
                    </div>
                </div>
    
                <script>
                    // Configuration
                    var POLL_INTERVAL = 2000;
                    var PRESENCE_INTERVAL = 15000;
                    
                    // State
                    var myName = localStorage.getItem('liquid_chat_name');
                    if (!myName) {
                        var randomId = Math.floor(1000 + Math.random() * 9000);
                        myName = 'anonymous-' + randomId;
                        localStorage.setItem('liquid_chat_name', myName);
                    }
                    
                    document.getElementById('your-name').textContent = myName;
                    
                    var lastMessageTime = 0;
                    var onlineCount = 1;
                    var isConnected = false;
                    var pollTimeout = null;
                    var messagesLoaded = false;
                    var messageIds = {};
                    
                    // DOM elements
                    var messagesEl = document.getElementById('messages');
                    var inputEl = document.getElementById('message-input');
                    var sendBtn = document.getElementById('send-button');
                    var connectionStatus = document.getElementById('connection-status');
                    var connectionText = document.getElementById('connection-text');
                    var onlineCountEl = document.getElementById('online-count');
                    
                    // Helper: Format time
                    function formatTime(timestamp) {
                        var date = new Date(timestamp);
                        var hours = date.getHours();
                        var minutes = date.getMinutes();
                        var ampm = hours >= 12 ? 'PM' : 'AM';
                        hours = hours % 12;
                        hours = hours ? hours : 12;
                        minutes = minutes < 10 ? '0' + minutes : minutes;
                        return hours + ':' + minutes + ' ' + ampm;
                    }
                    
                    // Helper: Clear messages
                    function clearMessages() {
                        messagesEl.innerHTML = '';
                        messageIds = {};
                    }
                    
                    // Helper: Add message to UI
                    function addMessage(message, isOwn, isMention) {
                        if (!message || !message.text) return;
                        
                        if (isOwn === undefined) isOwn = false;
                        if (isMention === undefined) isMention = false;
                        
                        // Check for duplicate using message ID
                        if (message.id && messageIds[message.id]) {
                            return;
                        }
                        
                        // Add to tracking set
                        if (message.id) {
                            messageIds[message.id] = true;
                        }
                        
                        var messageDiv = document.createElement('div');
                        messageDiv.className = 'message';
                        
                        if (message.type === 'system') {
                            messageDiv.classList.add('system');
                            messageDiv.textContent = message.text;
                        } else {
                            if (isOwn) messageDiv.classList.add('own');
                            if (isMention) messageDiv.classList.add('mention');
                            
                            var header = document.createElement('div');
                            header.className = 'message-header';
                            
                            var nameSpan = document.createElement('span');
                            nameSpan.textContent = message.from;
                            if (isOwn) {
                                var youSpan = document.createElement('span');
                                youSpan.className = 'you';
                                youSpan.textContent = ' (you)';
                                nameSpan.appendChild(youSpan);
                            }
                            header.appendChild(nameSpan);
                            
                            var content = document.createElement('div');
                            content.className = 'message-content';
                            content.textContent = message.text;
                            
                            var time = document.createElement('div');
                            time.className = 'message-time';
                            time.textContent = formatTime(message.ts || Date.now());
                            
                            messageDiv.appendChild(header);
                            messageDiv.appendChild(content);
                            messageDiv.appendChild(time);
                        }
                        
                        messagesEl.appendChild(messageDiv);
                        messagesEl.scrollTop = messagesEl.scrollHeight;
                    }
                    
                    // Helper: Add system message
                    function addSystemMessage(text) {
                        addMessage({ type: 'system', text: text }, false, false);
                    }
                    
                    // Load all messages (for new users)
                    function loadAllMessages() {
                        addSystemMessage('Loading chat history...');
                        
                        fetch('/chat/messages')
                            .then(function(response) {
                                if (!response.ok) {
                                    throw new Error('Failed to load messages');
                                }
                                return response.json();
                            })
                            .then(function(data) {
                                if (data.messages && data.messages.length > 0) {
                                    clearMessages();
                                    
                                    for (var i = 0; i < data.messages.length; i++) {
                                        var msg = data.messages[i];
                                        var isOwn = (msg.from === myName);
                                        var isMention = false;
                                        
                                        if (msg.mentions && Array.isArray(msg.mentions)) {
                                            isMention = msg.mentions.includes(myName);
                                        }
                                        
                                        addMessage(msg, isOwn, isMention);
                                        
                                        if (msg.ts > lastMessageTime) {
                                            lastMessageTime = msg.ts;
                                        }
                                    }
                                    
                                    addSystemMessage('Loaded ' + data.messages.length + ' messages from history');
                                    console.log('Loaded ' + data.messages.length + ' historical messages');
                                } else {
                                    addSystemMessage('No message history');
                                }
                                
                                messagesLoaded = true;
                            })
                            .catch(function(error) {
                                console.error('Failed to load messages:', error);
                                addSystemMessage('Failed to load message history');
                                messagesLoaded = true;
                            });
                    }
                    
                    // Fetch new messages (polling)
                    function fetchNewMessages() {
                        if (!messagesLoaded) return;
                        
                        fetch('/chat/messages?after=' + lastMessageTime)
                            .then(function(response) {
                                if (!response.ok) {
                                    throw new Error('Failed to fetch messages');
                                }
                                return response.json();
                            })
                            .then(function(data) {
                                if (data.messages && data.messages.length > 0) {
                                    for (var i = 0; i < data.messages.length; i++) {
                                        var msg = data.messages[i];
                                        
                                        if (msg.ts > lastMessageTime) {
                                            var isOwn = (msg.from === myName);
                                            var isMention = false;
                                            
                                            if (msg.mentions && Array.isArray(msg.mentions)) {
                                                isMention = msg.mentions.includes(myName);
                                            }
                                            
                                            addMessage(msg, isOwn, isMention);
                                            
                                            if (msg.ts > lastMessageTime) {
                                                lastMessageTime = msg.ts;
                                            }
                                        }
                                    }
                                }
                                
                                if (!isConnected) {
                                    isConnected = true;
                                    connectionStatus.className = 'connection-status connected';
                                    connectionText.textContent = 'Connected';
                                }
                            })
                            .catch(function(error) {
                                console.error('Poll error:', error);
                                
                                if (isConnected) {
                                    isConnected = false;
                                    connectionStatus.className = 'connection-status disconnected';
                                    connectionText.textContent = 'Disconnected (reconnecting...)';
                                }
                            })
                            .finally(function() {
                                pollTimeout = setTimeout(fetchNewMessages, POLL_INTERVAL);
                            });
                    }
                    
                    // Send message
                    function sendMessage() {
                        var text = inputEl.value.trim();
                        if (!text) return;
                        
                        inputEl.disabled = true;
                        sendBtn.disabled = true;
                        sendBtn.textContent = 'Sending...';
                        
                        fetch('/chat/send', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ 
                                from: myName, 
                                text: text,
                                timestamp: Date.now()
                            })
                        })
                        .then(function(response) {
                            if (!response.ok) {
                                return response.json().then(function(err) {
                                    throw new Error(err.error || 'Failed to send');
                                });
                            }
                            return response.json();
                        })
                        .then(function(result) {
                            inputEl.value = '';
                            
                            addMessage({
                                id: result.id,
                                from: myName,
                                text: text,
                                ts: result.ts || Date.now()
                            }, true, false);
                            
                            if (result.ts > lastMessageTime) {
                                lastMessageTime = result.ts;
                            }
                        })
                        .catch(function(error) {
                            console.error('Send error:', error);
                            addSystemMessage('⚠️ Failed to send message. Please try again.');
                        })
                        .finally(function() {
                            inputEl.disabled = false;
                            sendBtn.disabled = false;
                            sendBtn.textContent = 'Send';
                            inputEl.focus();
                        });
                    }
                    
                    // Update presence
                    function updatePresence() {
                        fetch('/chat/presence', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ from: myName })
                        })
                        .catch(function(error) {
                            console.error('Presence update error:', error);
                        });
                        
                        fetch('/chat/presence-count')
                            .then(function(response) {
                                if (response.ok) {
                                    return response.json();
                                }
                                throw new Error('Failed to get presence count');
                            })
                            .then(function(data) {
                                onlineCount = data.count || 1;
                                onlineCountEl.textContent = onlineCount + ' online';
                            })
                            .catch(function(error) {
                                console.error('Presence count error:', error);
                            });
                    }
                    
                    // Event listeners
                    sendBtn.addEventListener('click', sendMessage);
                    
                    inputEl.addEventListener('keypress', function(e) {
                        if (e.key === 'Enter' && !e.shiftKey) {
                            e.preventDefault();
                            sendMessage();
                        }
                    });
                    
                    // Initialize
                    function init() {
                        connectionStatus.className = 'connection-status connecting';
                        connectionText.textContent = 'Connecting...';
                        
                        loadAllMessages();
                        
                        fetchNewMessages();
                        
                        updatePresence();
                        setInterval(updatePresence, PRESENCE_INTERVAL);
                        
                        inputEl.focus();
                    }
                    
                    init();
                    
                    window.addEventListener('beforeunload', function() {
                        if (pollTimeout) {
                            clearTimeout(pollTimeout);
                        }
                    });
                </script>
            </body>
            </html>
        `);
    }

// Add presence count endpoint
if (url.pathname === '/chat/presence-count') {
    if (req.method !== 'GET') {
        res.setHeader('Allow', 'GET');
        res.setHeader('Access-Control-Allow-Origin', '*');
        return res.status(405).json({ error: 'MethodNotAllowed' });
    }

    try {
        const keys = await kv.keys('chat:presence:*');
        const count = keys?.length || 0;
        
        res.setHeader('Access-Control-Allow-Origin', '*');
        res.setHeader('Content-Type', 'application/json');
        return res.status(200).json({ count });
    } catch (e) {
        console.error('[CHAT] Presence count error:', e);
        res.setHeader('Access-Control-Allow-Origin', '*');
        return res.status(500).json({ error: 'InternalError' });
    }
}

    if (url.pathname === '/dashboard' || url.pathname === '/dash') {
        if (req.headers['x-requested-with'] === 'XMLHttpRequest') {
            let visitorCount = 0;
            let peakVisitors = 0;
            try {
                const visitorKeys = await kv.keys('visitor:*');
                visitorCount = visitorKeys.length;
                peakVisitors = await kv.get('peak_visitors') || 0;
                if (visitorCount > peakVisitors) {
                    await kv.set('peak_visitors', visitorCount);
                    peakVisitors = visitorCount;
                }
            } catch (e) {}

            const trackersCount = liquidWS.trackerSockets ? liquidWS.trackerSockets.size : 0;
            const users = liquidWS.userSockets ? Array.from(liquidWS.userSockets.values()).map(u => u.username) : [];

            return res.status(200).json({ 
                visitorCount, 
                peakVisitors, 
                trackersCount,
                liquids: users 
            });
        }

        res.setHeader('Content-Type', 'text/html');
        return res.status(200).send(`
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <title>LIQUID // DASH</title>
                <style>
                    body, html { margin: 0; padding: 0; height: 100%; width: 100%; overflow: hidden; background: #000; font-family: 'Inter', sans-serif; }
                    
                    /* THE VIDEO LAYER - HARD RESET */
                    #bg-fix {
                        position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
                        z-index: 1; background: #000;
                    }
                    video { width: 100%; height: 100%; object-fit: cover; filter: brightness(0.5); display: block; }

                    /* UI LAYER */
                    .ui {
                        position: absolute; top: 0; left: 0; width: 100%; height: 100%;
                        z-index: 2; display: flex; flex-direction: column; 
                        justify-content: center; align-items: center; pointer-events: none;
                    }

                    .boxes { display: flex; gap: 20px; margin-bottom: 20px; pointer-events: auto; }
                    .card {
                        background: rgba(0, 0, 0, 0.75); backdrop-filter: blur(20px);
                        border: 1px solid rgba(255, 255, 255, 0.1);
                        padding: 40px 60px; border-radius: 10px; text-align: center;
                    }

                    .label { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 4px; color: #888; margin-bottom: 5px; }
                    .val { font-size: 5rem; font-weight: 900; color: #fff; margin: 0; line-height: 1; }

                    .sub-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; width: 100%; max-width: 700px; pointer-events: auto; }
                    .mini { background: rgba(0, 0, 0, 0.5); border: 1px solid rgba(255,255,255,0.1); padding: 20px; border-radius: 8px; text-align: center; }

                    .badge { display: inline-block; background: #fff; color: #000; padding: 5px 12px; font-size: 0.7rem; font-weight: 900; margin: 3px; border-radius: 4px; }

                    /* NOTIFS - TOP RIGHT */
                    #notif-stack { position: fixed; top: 20px; right: 20px; z-index: 999; display: flex; flex-direction: column; gap: 10px; }
                    .toast {
                        background: #fff; color: #000; padding: 12px 25px; font-weight: 900; font-size: 0.75rem;
                        text-transform: uppercase; border-radius: 4px; box-shadow: 0 10px 30px rgba(0,0,0,0.5);
                        animation: slideIn 0.4s ease forwards;
                    }
                    @keyframes slideIn { from { transform: translateX(120%); } to { transform: translateX(0); } }

                    #audio-btn {
                        position: fixed; bottom: 20px; right: 20px; z-index: 999;
                        background: rgba(255,255,255,0.1); border: 1px solid #fff; color: #fff;
                        padding: 10px 20px; cursor: pointer; font-size: 0.7rem; font-weight: 900;
                    }
                </style>
            </head>
            <body>
                <div id="bg-fix">
                    <video id="v" autoplay muted loop playsinline>
                        <source src="https://consoletest-peach.vercel.app/files/hamburger.mp4" type="video/mp4">
                    </video>
                </div>

                <div id="notif-stack"></div>
                <button id="audio-btn" onclick="mute()">UNMUTE AUDIO</button>

                <div class="ui">
                    <div class="boxes">
                        <div class="card"><div class="label">Visitors</div><div class="val" id="c">0</div></div>
                        <div class="card"><div class="label">Peak</div><div class="val" id="p">0</div></div>
                    </div>
                    <div class="sub-grid">
                        <div class="mini"><div class="label">Trackers</div><div id="tc" style="font-size: 2rem; font-weight: 800;">0</div></div>
                        <div class="mini"><div class="label">Sockets</div><div id="ul" style="margin-top: 10px;">---</div></div>
                    </div>
                </div>

                <script>
                    const v = document.getElementById('v');
                    let prev = { c: 0, t: 0, u: 0 };

                    function mute() {
                        v.muted = !v.muted;
                        document.getElementById('audio-btn').innerText = v.muted ? "UNMUTE AUDIO" : "MUTE AUDIO";
                    }

                    function pop(txt) {
                        const s = document.getElementById('notif-stack');
                        const n = document.createElement('div');
                        n.className = 'toast'; n.innerText = txt;
                        s.appendChild(n);
                        setTimeout(() => n.remove(), 3000);
                    }

                    async function sync() {
                        try {
                            const r = await fetch(window.location.href, { headers: { 'x-requested-with': 'XMLHttpRequest' } });
                            const d = await r.json();
                            
                            if (d.visitorCount > prev.c) pop('New Visitor Connected');
                            if (d.trackersCount > prev.t) pop('Tracker Active');
                            if (d.liquids.length > prev.u) pop('Socket User Joined');

                            document.getElementById('c').innerText = d.visitorCount;
                            document.getElementById('p').innerText = d.peakVisitors;
                            document.getElementById('tc').innerText = d.trackersCount;
                            
                            const list = document.getElementById('ul');
                            list.innerHTML = d.liquids.length > 0 
                                ? d.liquids.map(u => \`<span class="badge">\${u}</span>\`).join('') 
                                : '<span style="color:#444">None</span>';

                            prev = { c: d.visitorCount, t: d.trackersCount, u: d.liquids.length };
                        } catch (e) {}
                    }
                    setInterval(sync, 2500);
                    sync();
                </script>
            </body>
            </html>
        `);
    }

    if (
        ['/data', '/json', '/serverdata'].includes(url.pathname) ||
        url.pathname.startsWith('/liquiddata')
      ) {
        let currentData = null;
      
        const fallbackPath = path.join(process.cwd(), 'api', 'data', 'data.json');
        if (fs.existsSync(fallbackPath)) {
          try {
            currentData = JSON.parse(fs.readFileSync(fallbackPath, 'utf8'));
            console.log('[DATA] Loaded from file');
          } catch (e) {
            console.error('[DATA] File parse error:', e);
          }
        }
      
        if (!currentData) {
          try {
            currentData = await kv.get('liquid_data');
            if (currentData) {
              console.log('[DATA] Loaded from KV');
            }
          } catch (e) {
            console.error('[DATA] KV error:', e);
          }
        }
      
        res.setHeader('Cache-Control', 'no-store');
        res.setHeader('Content-Type', 'application/json');
      
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

    return res.status(404).send(`
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <title>404 | NOT FOUND</title>
            <style>
                body { 
                    background: #000; color: #fff; font-family: 'Inter', sans-serif; 
                    display: flex; align-items: center; justify-content: center; 
                    height: 100vh; margin: 0; text-align: center;
                }
                .err-box { border: 1px solid #333; padding: 40px; border-radius: 12px; background: #050505; }
                h1 { font-size: 3rem; margin: 0; color: #ff3e3e; }
                p { color: #888; margin: 10px 0 20px; }
                .links { color: #fff; text-decoration: none; font-weight: bold; border-bottom: 1px solid #fff; margin: 0 10px; }
            </style>
        </head>
        <body>
            <div class="err-box">
                <h1>404</h1>
                <p>The URL you're looking for does not exist.</p>
                <div>
                    <a class="links" href="/dash">DASHBOARD</a>
                    <a class="links" href="/download">DOWNLOAD</a>
                </div>
            </div>
        </body>
        </html>
    `);

    function sendErrorPage(res, code, title, message) {
    res.setHeader('Content-Type', 'text/html');
    
    const subMessage = code === 405 
        ? "This endpoint is a logic gate. It only accepts <b>POST</b> requests from the system." 
        : message;

    return res.status(code).send(`
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <title>LIQUID // ${code}</title>
            <style>
                body { 
                    background: #000; color: #fff; font-family: 'Inter', sans-serif; 
                    display: flex; align-items: center; justify-content: center; 
                    height: 100vh; margin: 0; text-align: center;
                }
                .err-box { 
                    border: 1px solid rgba(255,255,255,0.1); padding: 50px; 
                    border-radius: 20px; background: rgba(255,255,255,0.02); 
                    backdrop-filter: blur(20px); max-width: 450px;
                }
                h1 { font-size: 5rem; margin: 0; color: #fff; letter-spacing: -4px; line-height: 0.9; }
                .status { color: #ff3e3e; font-weight: 900; letter-spacing: 3px; font-size: 0.7rem; margin-top: 10px; text-transform: uppercase; }
                p { color: #777; margin: 20px 0 40px; font-size: 0.85rem; line-height: 1.6; }
                b { color: #fff; border-bottom: 1px solid #fff; }
                .nav { display: flex; flex-wrap: wrap; justify-content: center; gap: 10px; }
                .links { 
                    color: #fff; text-decoration: none; font-size: 0.65rem; font-weight: 900; 
                    border: 1px solid rgba(255,255,255,0.2); padding: 12px 20px; 
                    border-radius: 6px; text-transform: uppercase; transition: 0.2s;
                }
                .links:hover { background: #fff; color: #000; border-color: #fff; }
            </style>
        </head>
        <body>
            <div class="err-box">
                <h1>${code}</h1>
                <div class="status">${title}</div>
                <p>${subMessage}</p>
                <div class="nav">
                    <a class="links" href="/dash">Dash</a>
                    <a class="links" href="/files">Files</a>
                    <a class="links" href="/download">Download</a>
                </div>
            </div>
        </body>
        </html>
    `);
}
}