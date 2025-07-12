const http = require('http');
const port = process.env.PORT || 8005;

const server = http.createServer((req, res) => {
  if (req.method === 'POST') {
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(body || '{}');
    });
    return;
  }
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('KSA MCP server placeholder');
});

server.listen(port, () => {
  console.log(`KSA MCP server listening on port ${port}`);
});
server.on('error', err => console.error(err));
