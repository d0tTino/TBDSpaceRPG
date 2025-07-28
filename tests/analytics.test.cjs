const fs = require('fs');
const path = require('path');
const assert = require('assert');
const analytics = require('../servers/telemetry/analytics.cjs');

const logPath = path.join(__dirname, 'analytics_test.log');
const emptyLogPath = path.join(__dirname, 'analytics_empty.log');
if (fs.existsSync(logPath)) fs.unlinkSync(logPath);
if (fs.existsSync(emptyLogPath)) fs.unlinkSync(emptyLogPath);

(async () => {
  try {
    const lines = [
      JSON.stringify({ type: 'test' }),
      'not json',
      JSON.stringify({ type: 'generation_advanced', generation: 3 })
    ].join('\n');
    await fs.promises.writeFile(logPath, lines);

    const logContents = await fs.promises.readFile(logPath, 'utf8');
    assert.ok(logContents.includes('test'));

    const metrics = analytics.parseLogFile(logPath);
    assert.strictEqual(metrics.totalEvents, 2);
    assert.strictEqual(metrics.eventCounts.test, 1);
    assert.strictEqual(metrics.eventCounts.generation_advanced, 1);
    assert.strictEqual(metrics.latestGeneration, 3);
    assert.strictEqual(metrics.mostRecentEventType, 'generation_advanced');
    assert.strictEqual(typeof metrics.averageEventsPerMinute, 'number');
    assert.ok(metrics.averageEventsPerMinute > 0);

    const metricsAgain = analytics.parseLogFile(logPath);
    assert.deepStrictEqual(metricsAgain, metrics);

    analytics.resetMetrics();
    analytics.computeMetrics({ type: 'generation_advanced', generation: 5 });
    const liveMetrics = analytics.getMetrics();
    assert.strictEqual(liveMetrics.latestGeneration, 5);
    assert.strictEqual(liveMetrics.mostRecentEventType, 'generation_advanced');

    // Parse an empty log file and ensure metrics stay zero
    await fs.promises.writeFile(emptyLogPath, '');
    const emptyMetrics = analytics.parseLogFile(emptyLogPath);
    assert.strictEqual(emptyMetrics.totalEvents, 0);
    assert.deepStrictEqual(emptyMetrics.eventCounts, {});
    assert.strictEqual(emptyMetrics.latestGeneration, null);
    assert.strictEqual(emptyMetrics.averageEventsPerMinute, 0);
    assert.strictEqual(emptyMetrics.mostRecentEventType, null);
    // parseLogFile resets internal metrics so getMetrics should match
    assert.deepStrictEqual(analytics.getMetrics(), emptyMetrics);

    console.log('Analytics tests passed');
  } finally {
    if (fs.existsSync(logPath)) fs.unlinkSync(logPath);
    if (fs.existsSync(emptyLogPath)) fs.unlinkSync(emptyLogPath);
  }
})();
