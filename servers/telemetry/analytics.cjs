const fs = require('fs');

const metrics = {
  totalEvents: 0,
  eventCounts: {},
  latestGeneration: null,
  averageEventsPerMinute: 0,
  mostRecentEventType: null,
  startTime: null,
  lastEventTime: null,
};

function resetMetrics() {
  metrics.totalEvents = 0;
  metrics.eventCounts = {};
  metrics.latestGeneration = null;
  metrics.averageEventsPerMinute = 0;
  metrics.mostRecentEventType = null;
  metrics.startTime = null;
  metrics.lastEventTime = null;
}

function updateMetrics(event) {
  if (!event || typeof event !== 'object') return;
  if (typeof event.type !== 'string') return;
  const now = event.timestamp ? new Date(event.timestamp).getTime() : Date.now();
  if (!metrics.startTime) metrics.startTime = now;
  metrics.lastEventTime = now;
  metrics.totalEvents += 1;
  metrics.eventCounts[event.type] = (metrics.eventCounts[event.type] || 0) + 1;
  metrics.mostRecentEventType = event.type;
  if (event.type === 'generation_advanced' && typeof event.generation === 'number') {
    metrics.latestGeneration = event.generation;
  }
  const durationMinutes = (metrics.lastEventTime - metrics.startTime) / 60000;
  metrics.averageEventsPerMinute = durationMinutes > 0 ? metrics.totalEvents / durationMinutes : metrics.totalEvents;
}

function computeMetrics(event) {
  updateMetrics(event);
}

function parseLogFile(filePath) {
  resetMetrics();
  if (fs.existsSync(filePath)) {
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
  }
  return getMetrics();
}

function getMetrics() {
  return {
    totalEvents: metrics.totalEvents,
    eventCounts: metrics.eventCounts,
    latestGeneration: metrics.latestGeneration,
    averageEventsPerMinute: metrics.averageEventsPerMinute,
    mostRecentEventType: metrics.mostRecentEventType,
  };
}

module.exports = {
  computeMetrics,
  parseLogFile,
  resetMetrics,
  getMetrics,
};
