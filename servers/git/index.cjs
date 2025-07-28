const http = require('http');
const { logError } = require('../utils.cjs');
const { git: defaultPort } = require('../ports.cjs');
const port = process.env.PORT || defaultPort;

function sendJson(res, obj) {
  res.writeHead(200, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify(obj));
}

const server = http.createServer((req, res) => {
  if (req.method === 'GET' && req.url === '/init') {
    sendJson(res, { initialized: true });
    return;
  }

  if (req.method === 'GET' && req.url === '/commit') {
    sendJson(res, { committed: true });
    return;
  }

  if (req.method === 'GET' && req.url === '/status') {
    sendJson(res, { status: 'clean' });
    return;
  }

  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('Git MCP server placeholder');
});

server.listen(port, () => {
  console.log(`Git MCP server listening on port ${port}`);
});
server.on('error', logError);
