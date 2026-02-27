import { kv } from '@vercel/kv';
import fs from 'fs';
import path from 'path';
import { put, list, del } from '@vercel/blob'; 
import dotenv from 'dotenv';
import { WebSocketDurable } from './durable/websocket.js';
import { handleDataManagement } from './data/data-manager.js';
import { uploadTrackingData, uploadS3RoomData } from './tracker/tracker-manager.js';

dotenv.config({ path: '.env.local' });

const liquidWS = new WebSocketDurable();

export default async function handler(req, res) {
    const url = new URL(req.url, `http://${req.headers.host}`);

	const providedKey = url.search.slice(1).trim(); 
    const REQUIRED_PASS = process.env.FILES_PASS || "IMLUDDOSIGMA843924858";
    const isAdmin = (providedKey === REQUIRED_PASS);

    try {
        const userIp = req.headers['x-forwarded-for'] || req.socket.remoteAddress;
        if (userIp && !url.pathname.includes('/tracker')) { 
            await kv.set(`visitor:${userIp}`, '1', { ex: 60 });
        }
    } catch (e) {
        console.error("KV Visitor tracking error:", e);
    }

    if (req.headers.upgrade === 'websocket') {
        return liquidWS.handleUpgrade(req.socket, req);
    }

    // --- FILE SERVER LOGIC ---
    if (url.pathname === '/files' || url.pathname === '/files/') {
        
        // 1. Handle Delete (Admin Only)
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

        // 2. Handle Upload (Admin Only)
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
                    addRandomSuffix: false, // EXACT FILENAMES
                    token: process.env.BLOB_READ_WRITE_TOKEN
                });
                return res.status(200).json(blob);
            } catch (e) {
                return res.status(500).json({ error: e.message });
            }
        }

        // 3. Handle List (Public)
        if (req.headers['x-requested-with'] === 'XMLHttpRequest') {
            try {
                const { blobs } = await list();
                return res.status(200).json(blobs);
            } catch (e) {
                return res.status(500).json([]);
            }
        }

        // 4. Serve UI
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
                <div id="v-bg"><video autoplay muted loop playsinline><source src="https://files.hamburbur.org/hamburger.mp4" type="video/mp4"></video></div>
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
        // --- START DATA INJECTION LOGIC ---
        let currentData = await kv.get('liquid_data');
        if (!currentData) {
            const fallbackPath = path.join(process.cwd(), 'api', 'data', 'data.json');
            if (fs.existsSync(fallbackPath)) {
                currentData = JSON.parse(fs.readFileSync(fallbackPath, 'utf8'));
            }
        }
        // --- END DATA INJECTION LOGIC ---

        const menuPath = path.join(process.cwd(), 'public', 'menu-download.html');
        if (fs.existsSync(menuPath)) {
            let html = fs.readFileSync(menuPath, 'utf8');
            
            // Inject the data so your frontend can actually see the version/status
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
                        <source src="https://files.hamburbur.org/hamburger.mp4" type="video/mp4">
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
}