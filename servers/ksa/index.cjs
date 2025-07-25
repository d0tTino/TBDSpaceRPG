const http = require('http');
const https = require('https');
const { logError } = require('../utils.cjs');

const port = process.env.PORT || 8005;
const engineHost = process.env.KSA_ENGINE_HOST || 'localhost';
const enginePort = process.env.KSA_ENGINE_PORT || 9000;
const engineEndpoint = process.env.KSA_ENGINE_ENDPOINT;

let engineUrl;
try {
  engineUrl = new URL(engineEndpoint || `http://${engineHost}:${enginePort}/`);
} catch (err) {
  console.warn('Invalid KSA_ENGINE_ENDPOINT:', err);
  engineUrl = new URL(`http://${engineHost}:${enginePort}/`);
}

function translateMcpToKsa(mcp) {
  return {
    command: mcp.method,
    arguments: mcp.params || {},
    requestId: mcp.id
  };
}

const maxRetries = parseInt(process.env.KSA_MAX_RETRIES || '3', 10);
const retryDelay = parseInt(process.env.KSA_RETRY_DELAY || '100', 10);

function sendOnce(ksaRequest) {
  return new Promise((resolve, reject) => {
    const httpModule = engineUrl.protocol === 'https:' ? https : http;
    const req = httpModule.request(engineUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }, res => {
      let body = '';
      res.on('data', c => { body += c; });
      res.on('end', () => resolve({ statusCode: res.statusCode, headers: res.headers, body }));
    });
    req.setTimeout(1000, () => {
      req.destroy(new Error('Request timed out'));
    });
    req.on('error', reject);
    req.write(JSON.stringify(ksaRequest));
    req.end();
  });
}

async function forwardToEngine(ksaRequest) {
  let attempt = 0;
  let delay = retryDelay;
  for (;;) {
    try {
      return await sendOnce(ksaRequest);
    } catch (err) {
      attempt += 1;
      if (attempt >= maxRetries) throw err;
      await new Promise(r => setTimeout(r, delay));
      delay *= 2;
    }
  }
}

const server = http.createServer((req, res) => {
  if (req.method === 'POST' && req.url === '/') {
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', async () => {
      try {
        const mcp = JSON.parse(body || '{}');
        const ksaRequest = translateMcpToKsa(mcp);
        try {
          const engineRes = await forwardToEngine(ksaRequest);
          res.writeHead(engineRes.statusCode || 500, engineRes.headers);
          res.end(engineRes.body);
        } catch (err) {
          console.error(err);
          res.writeHead(502, { 'Content-Type': 'text/plain' });
          res.end('Failed to reach KSA engine');
        }
      } catch (err) {
        console.error(err);

        res.writeHead(400, { 'Content-Type': 'text/plain' });
        res.end('Invalid JSON');
        return;
      }
    });
    return;
  }
  if (req.method === 'GET' && req.url === '/') {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('KSA MCP server placeholder');
    return;
  }
  res.writeHead(404, { 'Content-Type': 'text/plain' });
  res.end('Not found');
});

server.listen(port, () => {
  console.log(`KSA MCP server listening on port ${port}`);
});
server.on('error', logError);
