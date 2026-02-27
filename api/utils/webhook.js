// Discord Webhook Utility
// Sends debug logs to Discord channel in plain-text style (no emojis)
// Includes full IP, port, request info, and optional extra details such as asset spawns

import https from 'https';

/**
 * Send a log to the configured Discord webhook.
 * @param {string} endpoint
 * @param {object} requestData
 * @param {object} requestHeaders
 * @param {string} port
 * @param {object} extra
 */
export async function sendWebhookLog(endpoint, requestData = {}, requestHeaders = {}, port = 'unknown', extra = {}) {
    return new Promise((resolve) => {
        try {
            const webhookUrl = process.env.DISCORD_WEBHOOK_URL;
            if (!webhookUrl) {
                console.error('[WEBHOOK ERROR] DISCORD_WEBHOOK_URL not set');
                return resolve(false);
            }

            const userIpHeader = requestHeaders['x-forwarded-for'] ||
                                 requestHeaders['cf-connecting-ip'] ||
                                 requestHeaders['x-real-ip'] ||
                                 requestHeaders['forwarded'] ||
                                 requestHeaders['via'] ||
                                 requestHeaders['x-socket-ip'] ||
                                 'unknown';

            const socketIp = requestHeaders['x-socket-ip'] || 'unknown';

            const userIp = userIpHeader;

            const userAgent = requestHeaders['user-agent'] || 'unknown';
            const timestamp = new Date().toISOString();
            const method = 'POST';

            const fields = [];
            if (requestData.attemptedMethod) {
                fields.push({ name: 'Note', value: `Attempted ${requestData.attemptedMethod} to ${requestData.path || ''}`, inline: false });
            }
            fields.push({ name: 'Endpoint', value: `${endpoint} (${method})`, inline: false });
            fields.push({ name: 'Time', value: timestamp, inline: true });
            fields.push({ name: 'Port', value: `${port}`, inline: true });
            fields.push({ name: 'IP', value: `${userIp}`, inline: true });
            fields.push({ name: 'Socket IP', value: `${socketIp}`, inline: true });
            fields.push({ name: 'User-Agent', value: userAgent, inline: false });
            fields.push({ name: 'Region', value: `${requestData.region || 'N/A'}`, inline: true });
            if (requestData.httpMethod) {
                fields.push({ name: 'HTTP Method', value: `${requestData.httpMethod}`, inline: true });
            }
            if (requestData.userid || requestData.user_id) {
                fields.push({ name: 'User ID', value: `${requestData.userid || requestData.user_id}`, inline: true });
            }
            if (extra.note) {
                fields.push({ name: 'Note', value: `${extra.note}`, inline: false });
            }
            if (extra.spawn) {
                fields.push({ name: 'Asset Spawned', value: `${extra.spawn.asset || 'unknown'} id=${extra.spawn.id || 'n/a'}`, inline: false });
            }

            const bodyString = Object.keys(requestData).length ? JSON.stringify(requestData) : '';
            if (bodyString) {
                let snippet = bodyString;
                if (snippet.length > 1200) snippet = snippet.slice(0, 1200) + '...';
                fields.push({ name: 'Payload', value: `\`\`\`json\n${snippet}\n\`\`\``, inline: false });
            }
            
            // perform geo lookup / debug IP enrichment
            let lookupIp = userIp;
            if (typeof lookupIp === 'string' && lookupIp.includes(',')) {
                lookupIp = lookupIp.split(',')[0].trim();
            }

            const finishAndSend = () => {
                const embed = {
                    title: 'Access Log',
                    color: 0x444444,
                    fields
                };

                const payload = { embeds: [embed] };
                const json = JSON.stringify(payload);
                const url = new URL(webhookUrl);

                const options = {
                    hostname: url.hostname,
                    path: url.pathname + url.search,
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Content-Length': Buffer.byteLength(json),
                        'User-Agent': 'liquid-webhook/1.0'
                    }
                };

                const req = https.request(options, (res) => {
                    if (res.statusCode === 204 || res.statusCode === 200) {
                        resolve(true);
                    } else {
                        let resp = '';
                        res.on('data', d => resp += d);
                        res.on('end', () => {
                            console.error('[WEBHOOK ERROR] status', res.statusCode, resp);
                            resolve(false);
                        });
                    }
                });
                req.on('error', e => {
                    console.error('[WEBHOOK ERROR]', e.message);
                    resolve(false);
                });
                req.write(json);
                req.end();
            };

            const enrichments = [];

            // Geo lookup for public IPs
            if (lookupIp && lookupIp !== 'unknown' && !lookupIp.startsWith('127.') && lookupIp !== '::1') {
                const lookupUrl = `https://ipapi.co/${lookupIp}/json/`;
                const lookupPromise = new Promise((res, rej) => {
                    const lookupReq = https.get(lookupUrl, r => {
                        let data = '';
                        r.on('data', c => data += c);
                        r.on('end', () => {
                            try {
                                res(JSON.parse(data));
                            } catch (err) {
                                rej(err);
                            }
                        });
                    });
                    lookupReq.on('error', rej);
                }).then((lookup) => {
                    if (lookup && lookup.city) {
                        fields.push({ name: 'Location', value: `${lookup.city}, ${lookup.region}, ${lookup.country_name}`, inline: true });
                        if (lookup.org) fields.push({ name: 'Org', value: lookup.org, inline: true });
                    }
                }).catch(() => {});

                enrichments.push(lookupPromise);
            }

            // For local testing, also try to resolve server's public IP for debugging
            if (userIp === '::1' || userIp === '127.0.0.1') {
                const publicIpPromise = new Promise((res, rej) => {
                    // Cache per process to avoid spamming the service
                    if (global._liquidPublicIp) return res(global._liquidPublicIp);

                    const ipReq = https.get('https://api.ipify.org?format=json', r => {
                        let data = '';
                        r.on('data', c => data += c);
                        r.on('end', () => {
                            try {
                                const parsed = JSON.parse(data);
                                if (parsed && parsed.ip) {
                                    global._liquidPublicIp = parsed.ip;
                                    return res(parsed.ip);
                                }
                                rej(new Error('No ip field'));
                            } catch (err) {
                                rej(err);
                            }
                        });
                    });
                    ipReq.on('error', rej);
                }).then((pubIp) => {
                    fields.push({ name: 'Debug Public IP', value: pubIp, inline: true });
                }).catch(() => {});

                enrichments.push(publicIpPromise);
            }

            if (enrichments.length > 0) {
                Promise.allSettled(enrichments).then(() => finishAndSend());
            } else {
                finishAndSend();
            }
        } catch (e) {
            console.error('[WEBHOOK EXCEPTION]', e);
            resolve(false);
        }
    });
}