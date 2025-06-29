const http = require('http');
const port = process.env.PORT || 8080;

const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('Git MCP server placeholder');
});

server.listen(port, () => {
  console.log(`Git MCP server listening on port ${port}`);
});
server.on('error', (err) => console.error(err));
