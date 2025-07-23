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
    public async Task ExecuteMenuItemRespondsWithExecuted()
    {
        using var ws = new ClientWebSocket();
        var uri = new Uri($"ws://localhost:{_port}/");
        await ws.ConnectAsync(uri, CancellationToken.None);

        var reqJson = "{\"type\":\"execute_menu_item\",\"id\":\"42\",\"parameters\":{\"item\":\"open\"}}";
        var bytes = Encoding.UTF8.GetBytes(reqJson);
        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[1024];
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var resp = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var doc = JsonDocument.Parse(resp);
        Assert.That(doc.RootElement.GetProperty("id").GetString(), Is.EqualTo("42"));
        Assert.That(doc.RootElement.GetProperty("result").GetProperty("executed").GetString(), Is.EqualTo("open"));
    }

    [Test]
    public async Task InvalidJsonDoesNotCloseConnection()
    {
        using var ws = new ClientWebSocket();
        var uri = new Uri($"ws://localhost:{_port}/");
        await ws.ConnectAsync(uri, CancellationToken.None);

        var badBytes = Encoding.UTF8.GetBytes("notjson");
        await ws.SendAsync(badBytes, WebSocketMessageType.Text, true, CancellationToken.None);
        await Task.Delay(100);

        Assert.That(ws.State, Is.EqualTo(WebSocketState.Open));

        var reqJson = "{\"type\":\"execute_menu_item\",\"id\":\"99\",\"parameters\":{}}";
        var reqBytes = Encoding.UTF8.GetBytes(reqJson);
        await ws.SendAsync(reqBytes, WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[1024];
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var resp = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var doc = JsonDocument.Parse(resp);
        Assert.That(doc.RootElement.GetProperty("id").GetString(), Is.EqualTo("99"));
        Assert.That(doc.RootElement.GetProperty("result").GetProperty("executed").GetString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task NotifyMessageStoresMessage()
    {
        using var ws = new ClientWebSocket();
        var uri = new Uri($"ws://localhost:{_port}/");
        await ws.ConnectAsync(uri, CancellationToken.None);

        var reqJson = "{\"type\":\"notify_message\",\"id\":\"55\",\"parameters\":{\"message\":\"hello\"}}";
        var bytes = Encoding.UTF8.GetBytes(reqJson);
        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[1024];
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var resp = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var doc = JsonDocument.Parse(resp);
        Assert.That(doc.RootElement.GetProperty("id").GetString(), Is.EqualTo("55"));
        Assert.That(doc.RootElement.GetProperty("result").GetProperty("notified").GetBoolean(), Is.True);
        Assert.That(NotifyMessageHandler.LastMessage, Is.EqualTo("hello"));
    }

    [Test]
    public async Task CloseConnectionWorks()
    {
        using var ws = new ClientWebSocket();
        var uri = new Uri($"ws://localhost:{_port}/");
        await ws.ConnectAsync(uri, CancellationToken.None);

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None);

        Assert.That(ws.State, Is.EqualTo(WebSocketState.Closed));
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
