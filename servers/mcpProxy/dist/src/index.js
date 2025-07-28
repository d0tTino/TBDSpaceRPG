const http = require('http');
const { mcpproxy: defaultPort } = require('../../ports.cjs');

const port = process.env.PORT || defaultPort;

const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('MCP Proxy server placeholder');
});

server.listen(port, () => {
  console.log(`MCP Proxy server listening on port ${port}`);
});

server.on('error', err => console.error(err));
