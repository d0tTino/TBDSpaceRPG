using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Chickensoft.GoDotTest;
using Shouldly;

public class GodotServerIntegrationTests : TestClass {
    public GodotServerIntegrationTests(Node scene) : base(scene) { }

    [Test]
    public async Task ExecuteMenuItemAndShutdown() {
        int port = GetFreePort();
        using var server = new SimpleWebSocketServer(port);
        server.Start();

        var client = new McpClient { Endpoint = $"ws://localhost:{port}/" };
        TestScene.AddChild(client);
        var resp = await client.SendWebSocketCommand("open");

        resp.GetProperty("result").GetProperty("executed").GetString().ShouldBe("open");

        server.Stop();

        await Should.ThrowAsync<WebSocketException>(async () => {
            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri($"ws://localhost:{port}/"), CancellationToken.None);
        });
    }

    private static int GetFreePort() {
        var l = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        l.Start();
        int p = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return p;
    }
}
