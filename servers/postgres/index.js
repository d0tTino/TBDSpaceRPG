const http = require('http');
const port = process.env.PORT || 8003;

const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('Postgres MCP server placeholder');
});

server.listen(port, () => {
  console.log(`Postgres MCP server listening on port ${port}`);
});
