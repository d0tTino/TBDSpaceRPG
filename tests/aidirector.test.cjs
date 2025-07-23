const http = require('http');
const { spawn } = require('child_process');
const path = require('path');
const assert = require('assert');

function startServer(relativePath, port) {
  const fullPath = path.join(__dirname, '..', relativePath);
  const env = { ...process.env, PORT: String(port) };
  return spawn('node', [fullPath], { env });
}

function request(port, options = {}) {
  return new Promise((resolve, reject) => {
    const req = http.request({ hostname: 'localhost', port, ...options }, res => {
      let data = '';
      res.on('data', c => { data += c; });
      res.on('end', () => resolve({ statusCode: res.statusCode, body: data }));
    });
    req.on('error', reject);
    if (options.method === 'POST' && options.body !== undefined) {
      if (typeof options.body === 'string') req.write(options.body);
      else req.write(JSON.stringify(options.body));
    }
    req.end();
  });
}

(async () => {
  const port = 8101;
  const server = startServer('servers/aidirector/index.cjs', port);
  try {
    // Wait for server to be ready
    await new Promise(resolve => setTimeout(resolve, 100));
    const root = await request(port);
    assert.strictEqual(root.body, 'AI Director server placeholder');

    const cmd = await request(port, {
      method: 'POST',
      path: '/command',
      headers: { 'Content-Type': 'application/json' },
      body: { action: 'test' },
    });
    assert.strictEqual(cmd.statusCode, 200);
    assert.deepStrictEqual(JSON.parse(cmd.body), { received: { action: 'test' } });
    const bad = await request(port, {
      method: 'POST',
      path: '/command',
      headers: { 'Content-Type': 'application/json' },
      body: '{',
    });
    assert.strictEqual(bad.statusCode, 400);

    console.log('AI Director tests passed');
  } finally {
    server.kill();
  }
})();

