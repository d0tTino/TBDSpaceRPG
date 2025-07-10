const express = require('express');
const fs = require('fs');
const path = require('path');
const rfs = require('rotating-file-stream');
const analytics = require('./analytics.cjs');

const port = process.env.PORT || 8090;
const logDir = path.join(__dirname, 'logs');
fs.mkdirSync(logDir, { recursive: true });
const logStream = rfs.createStream('events.log', {
  interval: '1d',
  path: logDir,
});

function logEvent(event) {
  logStream.write(`${JSON.stringify(event)}\n`);
  analytics.computeMetrics(event);
}

const app = express();
app.use(express.json());

app.get('/', (req, res) => {
  res.send('Telemetry server placeholder');
});

app.post('/event', (req, res) => {
  const event = req.body;
  logEvent(event);
  res.status(200).send('Event received');
});

app.listen(port, () => {
  console.log(`Telemetry server listening on port ${port}`);
});
app.on('error', (err) => console.error(err));
