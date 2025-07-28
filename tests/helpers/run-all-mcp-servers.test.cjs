const { spawnSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const assert = require('assert');
const getFreePort = require('./port.cjs');

function findPowerShell() {
  if (!spawnSync('pwsh', ['-Command', '$PSVersionTable.PSVersion']).error) {
    return 'pwsh';
  }
  if (!spawnSync('powershell', ['-Command', '$PSVersionTable.PSVersion']).error) {
    return 'powershell';
  }
  return null;
}

(async () => {
  const shell = findPowerShell();
  if (!shell) {
    console.log('PowerShell not found, skipping run-all-mcp-servers test');
    process.exit(0);
  }

  const names = ['unity', 'git', 'postgres', 'telemetry', 'mcpproxy', 'ksa'];
  const ports = {};
  for (const name of names) {
    ports[name] = await getFreePort();
  }
  const config = {};
  for (const [name, port] of Object.entries(ports)) {
    config[name] = { port };
  }

  const tmpDir = fs.mkdtempSync(path.join(__dirname, 'mcp-'));
  const configPath = path.join(tmpDir, 'engine-config.json');
  fs.writeFileSync(configPath, JSON.stringify(config, null, 2));

  const root = path.join(__dirname, '..', '..');
  const runScript = path.join(root, 'run-all-mcp-servers.ps1');
  const stopScript = path.join(root, 'stop-all-mcp-servers.ps1');
  const pidDir = path.join(root, 'tmp', 'pids');

  if (fs.existsSync(pidDir)) {
    fs.rmSync(pidDir, { recursive: true, force: true });
  }

  const runResult = spawnSync(shell, ['-File', runScript, '-ConfigFile', configPath], {
    stdio: 'inherit'
  });
  assert.strictEqual(runResult.status, 0, 'Failed to start MCP servers');

  await new Promise(r => setTimeout(r, 1000));

  const pidFiles = ['unity.pid', 'git.pid', 'postgres.pid', 'telemetry.pid', 'proxy.pid', 'ksa.pid'];
  const pids = [];
  for (const file of pidFiles) {
    const full = path.join(pidDir, file);
    assert.ok(fs.existsSync(full), `${file} not created`);
    const pid = parseInt(fs.readFileSync(full, 'utf8'), 10);
    assert.ok(pid > 0, `Invalid pid in ${file}`);
    pids.push(pid);
  }

  const stopResult = spawnSync(shell, ['-File', stopScript, '-PidDirectory', pidDir], {
    stdio: 'inherit'
  });
  assert.strictEqual(stopResult.status, 0, 'Failed to stop MCP servers');

  for (const file of pidFiles) {
    assert.ok(!fs.existsSync(path.join(pidDir, file)), `${file} still exists`);
  }

  for (const pid of pids) {
    let running = true;
    try {
      process.kill(pid, 0);
    } catch {
      running = false;
    }
    assert.strictEqual(running, false, `Process ${pid} still running`);
  }

  fs.rmSync(tmpDir, { recursive: true, force: true });

  console.log('run-all-mcp-servers test passed');
})();
