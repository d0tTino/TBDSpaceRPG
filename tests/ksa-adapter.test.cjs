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

  // Engine failure (unreachable)
  const failPort = await getFreePort();
  const adapterFailPort = await getFreePort();
  const adapterFail = startServer('servers/ksa/index.cjs', adapterFailPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(failPort)
  });
  try {
    await waitForServer(adapterFailPort);
    const result = await post(adapterFailPort, { method: 'ping', id: '1' });
    assert.strictEqual(result.statusCode, 502);
  } finally {
    await stopServer(adapterFail);
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
  } finally {
    await stopServer(adapterBad);
    await new Promise(r => engine2.close(r));
  }

  console.log('KSA adapter tests passed');
})();
