const http = require('http');

const port = process.env.PORT || 8004;

const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('MCP Proxy server placeholder');
});

server.listen(port, () => {
  console.log(`MCP Proxy server listening on port ${port}`);
});

server.on('error', err => console.error(err));
