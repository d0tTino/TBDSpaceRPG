const http = require('http');
const assert = require('assert');
const ports = require('../servers/ports.cjs');

function post(body) {
  return new Promise((resolve, reject) => {
    const req = http.request({ hostname: 'localhost', port: ports.unity, path: '/mcp', method: 'POST', headers: {'Content-Type':'application/json'} }, res => {
      res.resume();
      res.on('end', resolve);
    });
    req.on('error', reject);
    req.write(JSON.stringify(body));
    req.end();
  });
}

(async () => {
  const received = [];
  const server = http.createServer((req, res) => {
    let body = '';
    req.on('data', c => body += c);
    req.on('end', () => { received.push(JSON.parse(body)); res.end('ok'); });
  });
  await new Promise(r => server.listen(ports.unity, r));
  try {
    await post({ method: 'execute_menu_item', params: { menuPath: 'Test/Path' }, id: '1' });
    await post({ method: 'notify_message', params: { message: 'hi', logType: 'Log' }, id: '2' });
    assert.strictEqual(received.length, 2);
    assert.strictEqual(received[0].method, 'execute_menu_item');
    assert.strictEqual(received[1].method, 'notify_message');
    console.log('MCP command tests passed');
  } finally {
    server.close();
  }
})();
