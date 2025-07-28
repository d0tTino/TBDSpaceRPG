const fs = require('fs');
const path = require('path');

const configPath = path.join(__dirname, '..', 'engine-config.json');
let ports = {};
try {
  const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
  for (const [name, value] of Object.entries(config)) {
    if (value && typeof value.port === 'number') {
      ports[name] = value.port;
    }
  }
} catch (err) {
  console.warn('Failed to read engine-config.json:', err);
}
module.exports = ports;
