const http = require('http');
const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');
const assert = require('assert');
const analytics = require('../servers/telemetry/analytics.cjs');

function startServer(relativePath, port) {
  const fullPath = path.join(__dirname, '..', relativePath);
  const env = { ...process.env, PORT: String(port) };
  return spawn('node', [fullPath], { env });
}

function request(port) {
  return new Promise((resolve, reject) => {
    const req = http.request({ hostname: 'localhost', port, path: '/' }, (res) => {
      let data = '';
      res.on('data', chunk => { data += chunk; });
      res.on('end', () => resolve(data));
    });
    req.on('error', reject);
    req.end();
  });
}

function post(port, pathName, data) {
  return new Promise((resolve, reject) => {
    const req = http.request({
      hostname: 'localhost',
      port,
      path: pathName,
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    }, (res) => {
      let body = '';
      res.on('data', chunk => { body += chunk; });
      res.on('end', () => resolve({ statusCode: res.statusCode, body }));
    });
    req.on('error', reject);
    req.write(JSON.stringify(data));
    req.end();
  });
}

(async () => {
  const gitPort = 8081;
  const pgPort = 8004;
  const telemetryPort = 8091;
  const ksaPort = 8006;
  const git = startServer('servers/git/index.cjs', gitPort);
  const pg = startServer('servers/postgres/index.cjs', pgPort);
  const telemetry = startServer('servers/telemetry/index.cjs', telemetryPort);
  const ksa = startServer('servers/ksa/index.cjs', ksaPort);
  const logPath = path.join(__dirname, '..', 'servers', 'telemetry', 'logs', 'events.log');
  if (fs.existsSync(logPath)) fs.unlinkSync(logPath);
  async function waitForServer(port, timeout = 5000) {
    const start = Date.now();
    let connected = false;
    while (!connected) {
      try {
        await request(port);
        connected = true;
      } catch (err) {
        if (Date.now() - start > timeout) {
          throw new Error(`Timeout waiting for server on port ${port}`);
        }
        await new Promise(r => setTimeout(r, 100));
      }
    }
  }

  await Promise.all([
    waitForServer(gitPort),
    waitForServer(pgPort),
    waitForServer(telemetryPort),
    waitForServer(ksaPort),
  ]);
  try {
    const gitResponse = await request(gitPort);
    assert.strictEqual(gitResponse, 'Git MCP server placeholder');
    const pgResponse = await request(pgPort);
    assert.strictEqual(pgResponse, 'Postgres MCP server placeholder');
    const telemetryResponse = await request(telemetryPort);
    assert.strictEqual(telemetryResponse, 'Telemetry server placeholder');
    const ksaResponse = await request(ksaPort);
    assert.strictEqual(ksaResponse, 'KSA MCP server placeholder');
    const echo = await post(ksaPort, '/', { echo: 'test' });
    assert.strictEqual(echo.statusCode, 200);
    assert.strictEqual(echo.body, JSON.stringify({ echo: 'test' }));
    const postResult = await post(telemetryPort, '/event', { type: 'test' });
    assert.strictEqual(postResult.statusCode, 200);

    const genEvent = { type: 'generation_advanced', generation: 1 };
    const genResult = await post(telemetryPort, '/event', genEvent);
    assert.strictEqual(genResult.statusCode, 200);
    await new Promise(r => setTimeout(r, 100));
    const logPath = path.join(__dirname, '..', 'servers', 'telemetry', 'logs', 'events.log');
    const logContents = fs.readFileSync(logPath, 'utf8');
    assert.ok(logContents.includes(JSON.stringify(genEvent)));

    const metrics = analytics.parseLogFile(logPath);
    assert.strictEqual(metrics.totalEvents, 2);
    assert.strictEqual(metrics.eventCounts.test, 1);
    assert.strictEqual(metrics.eventCounts.generation_advanced, 1);
    assert.strictEqual(metrics.latestGeneration, 1);
    console.log('Tests passed');
    git.kill();
    pg.kill();
    telemetry.kill();
    ksa.kill();
  } catch (err) {
    git.kill();
    pg.kill();
    telemetry.kill();
    ksa.kill();
    console.error(err);
    process.exit(1);
  }
})();
