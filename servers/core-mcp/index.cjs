const http = require('http');
const fs = require('fs');
const path = require('path');
const analytics = require('../telemetry/analytics.cjs');
const { parseJson, logError } = require('../utils.cjs');
const { core: defaultPort } = require('../ports.cjs');

const port = process.env.PORT || defaultPort || 8100;

// --- Git state ---

function handleGit(req, res, subUrl) {
  if (req.method === 'GET' && subUrl === '/') {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Git MCP server placeholder');
    return true;
  }
  if (req.method === 'GET' && subUrl === '/init') {
    sendJson(res, 200, { initialized: true });
    return true;
  }
  if (req.method === 'GET' && subUrl === '/commit') {
    sendJson(res, 200, { committed: true });
    return true;
  }
  if (req.method === 'GET' && subUrl === '/status') {
    sendJson(res, 200, { status: 'clean' });
    return true;
  }
  return false;
}

// --- Postgres state ---

const crew = [];
let nextId = 1;

function sendJson(res, code, obj) {
  res.writeHead(code, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify(obj));
}

function handleCrew(req, res, subUrl) {
  const idMatch = subUrl.match(/^\/crew\/(\d+)$/);
  if (req.method === 'GET' && subUrl === '/crew') {
    sendJson(res, 200, crew);
    return true;
  }
  if (req.method === 'POST' && subUrl === '/crew') {
    let body = '';
    req.on('data', c => { body += c; });
    req.on('end', () => {
      const data = parseJson(body);
      if (!data) return sendJson(res, 400, { error: 'invalid json' });
      const record = { id: nextId++, ...data };
      crew.push(record);
      sendJson(res, 200, record);
    });
    return true;
  }
  if (req.method === 'PUT' && idMatch) {
    let body = '';
    req.on('data', c => { body += c; });
    req.on('end', () => {
      const data = parseJson(body);
      if (!data) return sendJson(res, 400, { error: 'invalid json' });
      const id = Number(idMatch[1]);
      const item = crew.find(c => c.id === id);
      if (!item) return sendJson(res, 404, { error: 'not found' });
      Object.assign(item, data);
      sendJson(res, 200, item);
    });
    return true;
  }
  if (req.method === 'DELETE' && idMatch) {
    const id = Number(idMatch[1]);
    const idx = crew.findIndex(c => c.id === id);
    if (idx === -1) return sendJson(res, 404, { error: 'not found' });
    crew.splice(idx, 1);
    sendJson(res, 200, { deleted: true });
    return true;
  }
  return false;
}

function handlePostgres(req, res, subUrl) {
  if (req.method === 'GET' && subUrl === '/') {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Postgres MCP server placeholder');
    return true;
  }
  if (subUrl.startsWith('/crew')) {
    return handleCrew(req, res, subUrl);
  }
  return false;
}

// --- Telemetry state ---

const logDir = path.join(__dirname, '..', 'telemetry', 'logs');
fs.mkdirSync(logDir, { recursive: true });
const logFile = path.join(logDir, 'events.log');

function logEvent(event) {
  fs.appendFileSync(logFile, `${JSON.stringify(event)}\n`);
  analytics.computeMetrics(event);
}

function handleTelemetry(req, res, subUrl) {
  if (req.method === 'GET' && subUrl === '/') {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Telemetry server placeholder');
    return true;
  }
  if (req.method === 'GET' && subUrl === '/metrics') {
    sendJson(res, 200, analytics.getMetrics());
    return true;
  }
  if (req.method === 'POST' && subUrl === '/event') {
    let body = '';
    req.on('data', chunk => { body += chunk; });
    req.on('end', () => {
      const event = parseJson(body);
      if (!event) {
        res.writeHead(400, { 'Content-Type': 'text/plain' });
        res.end('Invalid JSON');
        return;
      }
      logEvent(event);
      res.writeHead(200, { 'Content-Type': 'text/plain' });
      res.end('Event received');
    });
    return true;
  }
  return false;
}

const server = http.createServer((req, res) => {
  try {
    if (req.url.startsWith('/git')) {
      const sub = req.url.substring(4) || '/';
      if (handleGit(req, res, sub)) return;
    } else if (req.url.startsWith('/postgres')) {
      const sub = req.url.substring(9) || '/';
      if (handlePostgres(req, res, sub)) return;
    } else if (req.url.startsWith('/telemetry')) {
      const sub = req.url.substring(10) || '/';
      if (handleTelemetry(req, res, sub)) return;
    }
    res.writeHead(404, { 'Content-Type': 'text/plain' });
    res.end('Not found');
  } catch (err) {
    logError(err);
    res.writeHead(500, { 'Content-Type': 'text/plain' });
    res.end('Server error');
  }
});

server.listen(port, () => {
  console.log(`Core MCP server listening on port ${port}`);
});
server.on('error', logError);
