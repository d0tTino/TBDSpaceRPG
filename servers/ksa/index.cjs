const http = require('http');
const { logError, parseJson } = require('../utils.cjs');

const port = process.env.PORT || 8005;

function translateMcpToKsa(mcp) {
  return {
    command: mcp.method,
    arguments: mcp.params || {},
    requestId: mcp.id
  };
}

const server = http.createServer((req, res) => {
  if (req.method === 'POST') {
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
      const mcp = parseJson(body);
      if (!mcp) {
        res.writeHead(400, { 'Content-Type': 'text/plain' });
        res.end('Invalid JSON');
        return;
      }
      const ksaRequest = translateMcpToKsa(mcp);
      const response = { requestId: ksaRequest.requestId, result: { received: ksaRequest } };
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify(response));
    });
    return;
  }
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('KSA MCP server placeholder');
});

server.listen(port, () => {
  console.log(`KSA MCP server listening on port ${port}`);
});
server.on('error', logError);
