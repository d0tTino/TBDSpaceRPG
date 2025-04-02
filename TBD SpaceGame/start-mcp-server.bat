@echo off
echo Starting MCP Server on port 8001...
cd "TBD SpaceGame\TBD SpaceGame\Library\PackageCache\com.gamelovers.mcp-unity@3acfb232f564\Server"
set UNITY_PORT=8001
node build/index.js
pause 