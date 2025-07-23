const http = require('http');
const { logError, parseJson } = require('../utils.cjs');
const port = process.env.PORT || 8003;

const crew = [];
let nextId = 1;

function sendJson(res, code, obj) {
  res.writeHead(code, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify(obj));
}

function handleCrew(req, res) {
  const idMatch = req.url.match(/^\/crew\/(\d+)$/);
  if (req.method === 'GET' && req.url === '/crew') {
    sendJson(res, 200, crew);
    return;
  }
  if (req.method === 'POST' && req.url === '/crew') {
    let body = '';
    req.on('data', c => { body += c; });
    req.on('end', () => {
      const data = parseJson(body);
      if (!data) {
        sendJson(res, 400, { error: 'invalid json' });
        return;
      }
      const record = { id: nextId++, ...data };
      crew.push(record);
      sendJson(res, 200, record);
    });
    return;
  }
  if (req.method === 'PUT' && idMatch) {
    let body = '';
    req.on('data', c => { body += c; });
    req.on('end', () => {
      const data = parseJson(body);
      if (!data) {
        sendJson(res, 400, { error: 'invalid json' });
        return;
      }
      const id = Number(idMatch[1]);
      const item = crew.find(c => c.id === id);
      if (!item) {
        sendJson(res, 404, { error: 'not found' });
        return;
      }
      Object.assign(item, data);
      sendJson(res, 200, item);
    });
    return;
  }
  if (req.method === 'DELETE' && idMatch) {
    const id = Number(idMatch[1]);
    const idx = crew.findIndex(c => c.id === id);
    if (idx === -1) {
      sendJson(res, 404, { error: 'not found' });
      return;
    }
    crew.splice(idx, 1);
    sendJson(res, 200, { deleted: true });
    return;
  }
  res.writeHead(404, { 'Content-Type': 'text/plain' });
  res.end('Not found');
}

const server = http.createServer((req, res) => {
  if (req.method === 'GET' && req.url === '/') {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Postgres MCP server placeholder');
    return;
  }

  if (req.url.startsWith('/crew')) {
    handleCrew(req, res);
    return;
  }

  res.writeHead(404, { 'Content-Type': 'text/plain' });
  res.end('Not found');
});

server.listen(port, () => {
  console.log(`Postgres MCP server listening on port ${port}`);
});
server.on('error', logError);
