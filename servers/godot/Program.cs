using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

public class McpRequest
{
    public string type { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public JsonElement parameters { get; set; }
}

public class McpResponse
{
    public string id { get; set; } = string.Empty;
    public object result { get; set; } = new { status = "ok" };
}

public class SimpleWebSocketServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private Task? _listenTask;

    public int Port { get; }

    public SimpleWebSocketServer(int port)
    {
        Port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://+:{port}/");
    }

    public void Start()
    {
        _listener.Start();
        _listenTask = Task.Run(() => ListenLoop(_cts.Token));
    }

    private async Task ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener.GetContextAsync();
            }
            catch (HttpListenerException) when (token.IsCancellationRequested)
            {
                return;
            }

            if (!ctx.Request.IsWebSocketRequest)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Close();
                continue;
            }

            var wsContext = await ctx.AcceptWebSocketAsync(null);
            _ = Task.Run(() => HandleConnection(wsContext.WebSocket, token));
        }
    }

    private async Task HandleConnection(WebSocket socket, CancellationToken token)
    {
        var buffer = new byte[1024];
        while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(buffer, token);
            }
            catch
            {
                break;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", token);
                break;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(json);
                if (request != null)
                {
                    var response = new McpResponse { id = request.id };
                    var respJson = JsonSerializer.Serialize(response);
                    var respBytes = Encoding.UTF8.GetBytes(respJson);
                    await socket.SendAsync(respBytes, WebSocketMessageType.Text, true, token);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to process packet: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
        try
        {
            _listenTask?.Wait();
        }
        catch { }
    }

    public void Dispose()
    {
        Stop();
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        if (!int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
            port = 8001;

        using var server = new SimpleWebSocketServer(port);
        server.Start();
        Console.WriteLine($"Godot MCP server listening on port {port}");
        Console.WriteLine("Press Enter to stop the server...");
        Console.ReadLine();
        server.Stop();
    }
}
