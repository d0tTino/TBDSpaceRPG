const http = require('http');
const fs = require('fs');
const path = require('path');
const analytics = require('./analytics.cjs');
const { logError, parseJson } = require('../utils.cjs');

const port = process.env.PORT || 8090;
const logDir = path.join(__dirname, 'logs');
fs.mkdirSync(logDir, { recursive: true });
const logFile = path.join(logDir, 'events.log');

function logEvent(event) {
  fs.appendFileSync(logFile, `${JSON.stringify(event)}\n`);
  analytics.computeMetrics(event);
}

const server = http.createServer((req, res) => {
  if (req.method === 'GET' && req.url === '/') {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Telemetry server placeholder');
    return;
  }

  if (req.method === 'GET' && req.url === '/metrics') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(analytics.getMetrics()));
    return;
  }

  if (req.method === 'POST' && req.url === '/event') {
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
    return;
  }

  res.writeHead(404, { 'Content-Type': 'text/plain' });
  res.end('Not found');
});

server.listen(port, () => {
  console.log(`Telemetry server listening on port ${port}`);
});
server.on('error', logError);
