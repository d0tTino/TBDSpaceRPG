const http = require('http');
const { spawn } = require('child_process');
const path = require('path');
const assert = require('assert');
const getFreePort = require('./helpers/port.cjs');

function waitForServer(port, timeout = 5000) {
  return new Promise((resolve, reject) => {
    const start = Date.now();
    (function check() {
      const req = http.request({ hostname: 'localhost', port }, res => {
        res.resume();
        resolve();
      });
      req.on('error', err => {
        if (Date.now() - start > timeout) {
          reject(err);
        } else {
          setTimeout(check, 100);
        }
      });
      req.end();
    })();
  });
}



function startServer(relativePath, port, extraEnv = {}) {
  const fullPath = path.join(__dirname, '..', relativePath);
  const env = { ...process.env, PORT: String(port), ...extraEnv };
  const child = spawn('node', [fullPath], { env, stdio: 'inherit' });
  return child;
}

function startServerCapture(relativePath, port, extraEnv = {}) {
  const fullPath = path.join(__dirname, '..', relativePath);
  const env = { ...process.env, PORT: String(port), ...extraEnv };
  const child = spawn('node', [fullPath], { env, stdio: ['ignore', 'pipe', 'pipe'] });
  child.stdout.setEncoding('utf8');
  child.stderr.setEncoding('utf8');
  return child;
}

async function stopServer(child) {
  if (child && child.exitCode === null) {
    child.kill();
    await new Promise(resolve => child.once('exit', resolve));
  }
}

function post(port, data, raw = false) {
  return new Promise((resolve, reject) => {
    const req = http.request({
      hostname: 'localhost',
      port,
      path: '/',
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }, res => {
      let body = '';
      res.on('data', c => { body += c; });
      res.on('end', () => resolve({ statusCode: res.statusCode, body }));
    });
    req.on('error', reject);
    if (raw) req.write(data); else req.write(JSON.stringify(data));
    req.end();
  });
}




(async () => {
  // Successful forwarding using endpoint URL
  const enginePort = await getFreePort();
  const adapterPort = await getFreePort();
  const received = [];
  const engine = http.createServer((req, res) => {
    let body = '';
    req.on('data', c => { body += c; });
    req.on('end', () => {
      received.push({ path: req.url, body: JSON.parse(body || '{}') });
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ ok: true }));
    });
  });
  await new Promise(r => engine.listen(enginePort, r));
  const adapter = startServer('servers/ksa/index.cjs', adapterPort, {
    KSA_ENGINE_ENDPOINT: `http://localhost:${enginePort}/engine`
  });
  try {
    await waitForServer(adapterPort);
    const mcp = { method: 'notify_message', params: { msg: 'hello' }, id: '99' };
    const result = await post(adapterPort, mcp);
    assert.strictEqual(result.statusCode, 200);
    assert.deepStrictEqual(JSON.parse(result.body), { ok: true });
    assert.deepStrictEqual(received[0], {
      path: '/engine',
      body: {
        command: 'notify_message',
        arguments: { msg: 'hello' },
        requestId: '99'
      }
    });
  } finally {
    await stopServer(adapter);
    await new Promise(r => engine.close(r));
  }

  // Engine starts after delay to test retry logic
  const latePort = await getFreePort();
  const adapterRetryPort = await getFreePort();
  const adapterRetry = startServer('servers/ksa/index.cjs', adapterRetryPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(latePort),
    KSA_MAX_RETRIES: '5',
    KSA_RETRY_DELAY: '100'
  });
  let engineLate;
  try {
    await waitForServer(adapterRetryPort);
    const start = Date.now();
    const responsePromise = post(adapterRetryPort, { method: 'ping', id: '2' });
    await new Promise(r => setTimeout(r, 300));
    engineLate = http.createServer((req, res) => {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end('{}');
    });
    await new Promise(r => engineLate.listen(latePort, r));
    const result = await responsePromise;
    const duration = Date.now() - start;
    assert.strictEqual(result.statusCode, 200);
    assert.ok(duration >= 300);
  } finally {
    if (engineLate) await new Promise(r => engineLate.close(r));
    await stopServer(adapterRetry);
  }

  // Engine failure (unreachable)
  const failPort = await getFreePort();
  const adapterFailPort = await getFreePort();
  const adapterFail = startServer('servers/ksa/index.cjs', adapterFailPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(failPort)
  });
  try {
    await waitForServer(adapterFailPort);
    const startFail = Date.now();
    const result = await post(adapterFailPort, { method: 'ping', id: '1' });
    const durationFail = Date.now() - startFail;
    assert.strictEqual(result.statusCode, 502);
    assert.ok(durationFail >= 200);
  } finally {
    await stopServer(adapterFail);
  }

  // Invalid payload (schema validation)
  const engine3Port = await getFreePort();
  const engine3 = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end('{}');
  });
  await new Promise(r => engine3.listen(engine3Port, r));
  const adapterInvalidPort = await getFreePort();
  const adapterInvalid = startServer('servers/ksa/index.cjs', adapterInvalidPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(engine3Port)
  });
  try {
    await waitForServer(adapterInvalidPort);
    const invalidRes = await post(adapterInvalidPort, { method: 'ping' });
    assert.strictEqual(invalidRes.statusCode, 400);
    assert.ok(/Invalid request/.test(invalidRes.body));
  } finally {
    await stopServer(adapterInvalid);
    await new Promise(r => engine3.close(r));
  }

  // Malformed request
  const engine2Port = await getFreePort();
  const engine2 = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end('{}');
  });
  await new Promise(r => engine2.listen(engine2Port, r));
  const adapterBadPort = await getFreePort();
  const adapterBad = startServer('servers/ksa/index.cjs', adapterBadPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(engine2Port)
  });
  try {
    await waitForServer(adapterBadPort);
    const badRes = await post(adapterBadPort, '{', true);
    assert.strictEqual(badRes.statusCode, 400);
    assert.ok(/Invalid JSON/.test(badRes.body));
    const emptyRes = await post(adapterBadPort, '', true);
    assert.strictEqual(emptyRes.statusCode, 400);
  } finally {
    await stopServer(adapterBad);
    await new Promise(r => engine2.close(r));
  }

  // Missing engine host/port should use defaults and warn
  const defaultEngine = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end('{}');
  });
  await new Promise(r => defaultEngine.listen(9000, r));
  const defaultAdapterPort = await getFreePort();
  const adapterDefault = startServerCapture('servers/ksa/index.cjs', defaultAdapterPort);
  const hostWarnings = [];
  adapterDefault.stderr.on('data', d => hostWarnings.push(d));
  try {
    await waitForServer(defaultAdapterPort);
    const resDefault = await post(defaultAdapterPort, { method: 'ping', id: '7' });
    assert.strictEqual(resDefault.statusCode, 200);
    const warningText = hostWarnings.join('');
    assert.ok(/KSA_ENGINE_HOST/.test(warningText));
    assert.ok(/KSA_ENGINE_PORT/.test(warningText));
  } finally {
    await stopServer(adapterDefault);
    await new Promise(r => defaultEngine.close(r));
  }

  // Invalid KSA_MAX_RETRIES should fall back to default
  const badRetriesPort = await getFreePort();
  const adapterRetriesPort = await getFreePort();
  const adapterRetries = startServerCapture('servers/ksa/index.cjs', adapterRetriesPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(badRetriesPort),
    KSA_MAX_RETRIES: 'abc',
    KSA_RETRY_DELAY: '10'
  });
  const retryWarnings = [];
  adapterRetries.stderr.on('data', d => retryWarnings.push(d));
  try {
    await waitForServer(adapterRetriesPort);
    const tStart = Date.now();
    const resultRetries = await post(adapterRetriesPort, { method: 'ping', id: '8' });
    const dur = Date.now() - tStart;
    assert.strictEqual(resultRetries.statusCode, 502);
    assert.ok(dur >= 20);
    assert.ok(retryWarnings.join('').includes('KSA_MAX_RETRIES'));
  } finally {
    await stopServer(adapterRetries);
  }

  console.log('KSA adapter tests passed');
})();
