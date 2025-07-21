using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace GodotServer.Tests;

public class WebSocketTests
{
    private SimpleWebSocketServer? _server;
    private int _port;

    [SetUp]
    public void Setup()
    {
        _port = GetFreePort();
        _server = new SimpleWebSocketServer(_port);
        _server.Start();
    }

    [TearDown]
    public void Teardown()
    {
        _server?.Dispose();
    }

    [Test]
    public async Task HandshakeSucceeds()
    {
        using var ws = new ClientWebSocket();
        var uri = new Uri($"ws://localhost:{_port}/");
        await ws.ConnectAsync(uri, CancellationToken.None);
        Assert.That(ws.State, Is.EqualTo(WebSocketState.Open));
    }

    [Test]
    public async Task RequestRespondsWithOk()
    {
        using var ws = new ClientWebSocket();
        var uri = new Uri($"ws://localhost:{_port}/");
        await ws.ConnectAsync(uri, CancellationToken.None);

        var reqJson = "{\"type\":\"test\",\"id\":\"42\",\"parameters\":{}}";
        var bytes = Encoding.UTF8.GetBytes(reqJson);
        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[1024];
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var resp = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var doc = JsonDocument.Parse(resp);
        Assert.That(doc.RootElement.GetProperty("id").GetString(), Is.EqualTo("42"));
        Assert.That(doc.RootElement.GetProperty("result").GetProperty("status").GetString(), Is.EqualTo("ok"));
    }

    private static int GetFreePort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
