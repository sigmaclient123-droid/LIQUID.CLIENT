import https from 'https';
import dotenv from 'dotenv';

dotenv.config({ path: '.env.local' });

const webhookUrl = process.env.DISCORD_WEBHOOK_URL;
if (!webhookUrl) {
    console.error('DISCORD_WEBHOOK_URL missing, set it in .env.local');
    process.exit(1);
}

console.log('Using webhook:', webhookUrl.substring(0,60) + '...');

const payload = JSON.stringify({content: 'Standalone test ping from webhook-test.js'});
const url = new URL(webhookUrl);

const options = {
    hostname: url.hostname,
    path: url.pathname + url.search,
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(payload)
    }
};

const req = https.request(options, (res) => {
    console.log('statusCode', res.statusCode);
    let data = '';
    res.on('data', d => data += d);
    res.on('end', () => {
        console.log('body', data);
        process.exit(0);
    });
});

req.on('error', (e) => {
    console.error('error', e);
    process.exit(1);
});

req.write(payload);
req.end();
