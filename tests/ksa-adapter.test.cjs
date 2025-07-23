const http = require('http');
const { spawn } = require('child_process');
const path = require('path');
const assert = require('assert');

function startServer(relativePath, port, extraEnv = {}) {
  const fullPath = path.join(__dirname, '..', relativePath);
  const env = { ...process.env, PORT: String(port), ...extraEnv };
  return spawn('node', [fullPath], { env });
}

function post(port, data) {
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
    req.write(JSON.stringify(data));
    req.end();
  });
}

(async () => {
  const enginePort = 9010;
  const adapterPort = 9011;
  const received = [];
  const engine = http.createServer((req, res) => {
    let body = '';
    req.on('data', c => { body += c; });
    req.on('end', () => {
      received.push(JSON.parse(body || '{}'));
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ ok: true }));
    });
  });
  await new Promise(r => engine.listen(enginePort, r));
  const adapter = startServer('servers/ksa/index.cjs', adapterPort, {
    KSA_ENGINE_HOST: 'localhost',
    KSA_ENGINE_PORT: String(enginePort)
  });
  try {
    await new Promise(r => setTimeout(r, 100));
    const mcp = { method: 'notify_message', params: { msg: 'hello' }, id: '99' };
    const result = await post(adapterPort, mcp);
    assert.strictEqual(result.statusCode, 200);
    assert.deepStrictEqual(JSON.parse(result.body), { ok: true });
    assert.deepStrictEqual(received[0], {
      command: 'notify_message',
      arguments: { msg: 'hello' },
      requestId: '99'
    });
    console.log('KSA adapter tests passed');
  } finally {
    adapter.kill();
    engine.close();
  }
})();

