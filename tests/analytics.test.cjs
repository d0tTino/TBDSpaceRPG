const fs = require('fs');
const path = require('path');
const assert = require('assert');
const analytics = require('../servers/telemetry/analytics.cjs');

const logPath = path.join(__dirname, 'analytics_test.log');
if (fs.existsSync(logPath)) fs.unlinkSync(logPath);

(async () => {
  try {
    const lines = [
      JSON.stringify({ type: 'test' }),
      'not json',
      JSON.stringify({ type: 'generation_advanced', generation: 3 })
    ].join('\n');
    fs.writeFileSync(logPath, lines);

    const metrics = analytics.parseLogFile(logPath);
    assert.strictEqual(metrics.totalEvents, 2);
    assert.strictEqual(metrics.eventCounts.test, 1);
    assert.strictEqual(metrics.eventCounts.generation_advanced, 1);
    assert.strictEqual(metrics.latestGeneration, 3);

    const metricsAgain = analytics.parseLogFile(logPath);
    assert.deepStrictEqual(metricsAgain, metrics);

    analytics.resetMetrics();
    analytics.computeMetrics({ type: 'generation_advanced', generation: 5 });
    assert.strictEqual(analytics.getMetrics().latestGeneration, 5);

    console.log('Analytics tests passed');
  } finally {
    if (fs.existsSync(logPath)) fs.unlinkSync(logPath);
  }
})();
