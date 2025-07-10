const http = require('http');
const { spawn } = require('child_process');
const path = require('path');
const assert = require('assert');

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

(async () => {
  const gitPort = 8081;
  const pgPort = 8004;
  const git = startServer('servers/git/index.cjs', gitPort);
  const pg = startServer('servers/postgres/index.cjs', pgPort);
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
  ]);
  try {
    const gitResponse = await request(gitPort);
    assert.strictEqual(gitResponse, 'Git MCP server placeholder');
    const pgResponse = await request(pgPort);
    assert.strictEqual(pgResponse, 'Postgres MCP server placeholder');
    console.log('Tests passed');
    git.kill();
    pg.kill();
  } catch (err) {
    git.kill();
    pg.kill();
    console.error(err);
    process.exit(1);
  }
})();
