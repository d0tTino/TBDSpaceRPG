const fs = require('fs');

const metrics = {
  totalEvents: 0,
  eventCounts: {},
  latestGeneration: null,
};

function resetMetrics() {
  metrics.totalEvents = 0;
  metrics.eventCounts = {};
  metrics.latestGeneration = null;
}

function updateMetrics(event) {
  if (!event || typeof event !== 'object') return;
  if (typeof event.type !== 'string') return;
  metrics.totalEvents += 1;
  metrics.eventCounts[event.type] = (metrics.eventCounts[event.type] || 0) + 1;
  if (event.type === 'generation_advanced' && typeof event.generation === 'number') {
    metrics.latestGeneration = event.generation;
  }
}

function computeMetrics(event) {
  updateMetrics(event);
}

function parseLogFile(filePath) {
  resetMetrics();
  if (!fs.existsSync(filePath)) return metrics;
  const contents = fs.readFileSync(filePath, 'utf8');
  const lines = contents.split(/\r?\n/).filter(Boolean);
  for (const line of lines) {
    try {
      const evt = JSON.parse(line);
      updateMetrics(evt);
    } catch {
      // ignore invalid json
    }
  }
  return metrics;
}

function getMetrics() {
  return metrics;
}

module.exports = {
  computeMetrics,
  parseLogFile,
  resetMetrics,
  getMetrics,
};
