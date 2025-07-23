const http = require('http');

const port = process.env.PORT || 8005;
const engineHost = process.env.KSA_ENGINE_HOST || 'localhost';
const enginePort = process.env.KSA_ENGINE_PORT || 9000;

function translateMcpToKsa(mcp) {
  return {
    command: mcp.method,
    arguments: mcp.params || {},
    requestId: mcp.id
  };
}

function forwardToEngine(ksaRequest) {
  return new Promise((resolve, reject) => {
    const req = http.request({
      hostname: engineHost,
      port: enginePort,
      path: '/',
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }, res => {
      let body = '';
      res.on('data', c => { body += c; });
      res.on('end', () => resolve({ statusCode: res.statusCode, headers: res.headers, body }));
    });
    req.on('error', reject);
    req.write(JSON.stringify(ksaRequest));
    req.end();
  });
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
server.on('error', err => console.error(err));
