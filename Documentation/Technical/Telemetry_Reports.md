# Telemetry Reports

The telemetry server records gameplay events in `servers/telemetry/logs/events.log`. The analytics
module aggregates these events to provide high level metrics useful for debugging and balancing the game.

## Metrics
- **totalEvents** – number of events processed.
- **eventCounts** – count of each event type.
- **latestGeneration** – highest `generation` value seen in `generation_advanced` events.
- **averageEventsPerMinute** – events processed divided by minutes since the first event.
- **mostRecentEventType** – `type` of the last processed event.

Developers can parse a log file directly:

```javascript
const analytics = require('../../servers/telemetry/analytics.cjs');
const metrics = analytics.parseLogFile('servers/telemetry/logs/events.log');
console.log(metrics);
```

These metrics are updated live whenever the telemetry server receives an event.
