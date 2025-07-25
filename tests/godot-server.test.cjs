const { spawn, spawnSync } = require('child_process');
const path = require('path');
const assert = require('assert');
const WS = require('ws');
const getFreePort = require('./helpers/port.cjs');

if (spawnSync('dotnet', ['--version']).error) {
  console.log('dotnet not found, skipping Godot server test');
  process.exit(0);
}

function waitForWS(port, timeout = 5000) {
  return new Promise((resolve, reject) => {
    const start = Date.now();
    (function attempt() {
      const ws = new WS(`ws://localhost:${port}/`);
      ws.once('open', () => {
        ws.close();
        resolve();
      });
      ws.once('error', err => {
        if (Date.now() - start > timeout) {
          reject(err);
        } else {
          setTimeout(attempt, 100);
        }
      });
    })();
  });
}

(async () => {
  const port = await getFreePort();
  const projPath = path.join(__dirname, '..', 'servers', 'godot', 'GodotServer.csproj');
  const server = spawn('dotnet', ['run', '--project', projPath, '--no-build'], {
    env: { ...process.env, PORT: String(port) },
    stdio: 'inherit'
  });
  try {
    await waitForWS(port);
    const ws = new WS(`ws://localhost:${port}/`);
    await new Promise((resolve, reject) => {
      ws.once('open', resolve);
      ws.once('error', reject);
    });
    const item = 'MCP/Test';
    const id = '1';
    ws.send(JSON.stringify({ type: 'execute_menu_item', id, parameters: { item } }));
    const resp = await new Promise((resolve, reject) => {
      ws.once('message', data => {
        try {
          resolve(JSON.parse(data));
        } catch (e) {
          reject(e);
        }
      });
      ws.once('error', reject);
    });
    assert.deepStrictEqual(resp.result, { executed: item });
    ws.close();
    console.log('Godot server tests passed');
  } finally {
    server.kill();
  }
})();
