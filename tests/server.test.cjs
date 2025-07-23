const http = require('http');
const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');
const assert = require('assert');
const analytics = require('../servers/telemetry/analytics.cjs');

function startServer(relativePath, port, extraEnv = {}) {
  const fullPath = path.join(__dirname, '..', relativePath);
  const env = { ...process.env, PORT: String(port), ...extraEnv };
  return spawn('node', [fullPath], { env });
}

function request(port, pathName = '/') {
  return new Promise((resolve, reject) => {
    const req = http.request({ hostname: 'localhost', port, path: pathName }, res => {
      let data = '';
      res.on('data', chunk => { data += chunk; });
      res.on('end', () => resolve(data));
    });
    req.on('error', reject);
    req.end();
  });
}

function send(port, method, pathName, data) {
  return new Promise((resolve, reject) => {
    const req = http.request({
      hostname: 'localhost',
      port,
      path: pathName,
      method,
      headers: { 'Content-Type': 'application/json' },
    }, res => {
      let body = '';
      res.on('data', chunk => { body += chunk; });
      res.on('end', () => resolve({ statusCode: res.statusCode, body }));
    });
    req.on('error', reject);
    if (data !== undefined) {
      if (typeof data === 'string') req.write(data);
      else req.write(JSON.stringify(data));
    }
    req.end();
  });
}

function post(port, pathName, data) {
  return send(port, 'POST', pathName, data);
}

(async () => {
  const gitPort = 8081;
  const pgPort = 8004;
  const telemetryPort = 8091;
  const ksaPort = 8006;
  const ksaEnginePort = 8007;
  const ksaEngine = http.createServer((req, res) => {
    let body = '';
    req.on('data', c => { body += c; });
    req.on('end', () => {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(body);
    });
  });
  await new Promise(r => ksaEngine.listen(ksaEnginePort, r));
  const git = startServer('servers/git/index.cjs', gitPort);
  const pg = startServer('servers/postgres/index.cjs', pgPort);
  const telemetry = startServer('servers/telemetry/index.cjs', telemetryPort);
  const ksa = startServer('servers/ksa/index.cjs', ksaPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(ksaEnginePort)
  });
  const logPath = path.join(__dirname, '..', 'servers', 'telemetry', 'logs', 'events.log');
  if (fs.existsSync(logPath)) fs.unlinkSync(logPath);
  async function waitForServer(port, timeout = 5000) {
    const start = Date.now();
    let connected = false;
    while (!connected) {
      try {
        await request(port);
        connected = true;
      } catch {
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
    const gitInit = await request(gitPort, '/init');
    assert.deepStrictEqual(JSON.parse(gitInit), { initialized: true });
    const gitCommit = await request(gitPort, '/commit');
    assert.deepStrictEqual(JSON.parse(gitCommit), { committed: true });
    const gitStatus = await request(gitPort, '/status');
    assert.deepStrictEqual(JSON.parse(gitStatus), { status: 'clean' });
    const pgResponse = await request(pgPort);
    assert.strictEqual(pgResponse, 'Postgres MCP server placeholder');
    const created = await send(pgPort, 'POST', '/crew', { name: 'Alice', role: 'captain' });
    const crewItem = JSON.parse(created.body);
    assert.strictEqual(created.statusCode, 200);
    const list1 = await request(pgPort, '/crew');
    assert.deepStrictEqual(JSON.parse(list1), [crewItem]);
    const updated = await send(pgPort, 'PUT', `/crew/${crewItem.id}`, { role: 'pilot' });
    assert.strictEqual(updated.statusCode, 200);
    const updatedItem = JSON.parse(updated.body);
    assert.strictEqual(updatedItem.role, 'pilot');
    const deleted = await send(pgPort, 'DELETE', `/crew/${crewItem.id}`);
    assert.strictEqual(deleted.statusCode, 200);
    const list2 = await request(pgPort, '/crew');
    assert.deepStrictEqual(JSON.parse(list2), []);
    const badCreate = await send(pgPort, 'POST', '/crew', '{');
    assert.strictEqual(badCreate.statusCode, 400);
    const badUpdate = await send(pgPort, 'PUT', '/crew/1', '{');
    assert.strictEqual(badUpdate.statusCode, 400);
    const telemetryResponse = await request(telemetryPort);
    assert.strictEqual(telemetryResponse, 'Telemetry server placeholder');
    const ksaResponse = await request(ksaPort);
    assert.strictEqual(ksaResponse, 'KSA MCP server placeholder');
    const mcpCmd = { method: 'notify_message', params: { message: 'hi', logType: 'Log' }, id: '42' };
    const ksaResult = await post(ksaPort, '/', mcpCmd);
    assert.strictEqual(ksaResult.statusCode, 200);
    const expectedKsa = { command: 'notify_message', arguments: { message: 'hi', logType: 'Log' }, requestId: '42' };
    assert.deepStrictEqual(JSON.parse(ksaResult.body), expectedKsa);

    const postResult = await post(telemetryPort, '/event', { type: 'test' });
    assert.strictEqual(postResult.statusCode, 200);
    const badTelemetry = await post(telemetryPort, '/event', '{');
    assert.strictEqual(badTelemetry.statusCode, 400);

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

    const metrics2 = analytics.parseLogFile(logPath);
    assert.strictEqual(metrics2.totalEvents, 2);
    assert.strictEqual(metrics2.eventCounts.test, 1);
    assert.strictEqual(metrics2.eventCounts.generation_advanced, 1);
    assert.strictEqual(metrics2.latestGeneration, 1);

    const metricsResult = await request(telemetryPort, '/metrics');
    assert.deepStrictEqual(JSON.parse(metricsResult), metrics2);
    console.log('Tests passed');
    git.kill();
    pg.kill();
    telemetry.kill();
    ksa.kill();
    ksaEngine.close();
  } catch (err) {
    git.kill();
    pg.kill();
    telemetry.kill();
    ksa.kill();
    ksaEngine.close();
    console.error(err);
    process.exit(1);
  }
})();
