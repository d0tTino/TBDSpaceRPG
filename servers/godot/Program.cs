using System;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

public class McpBehavior : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            var request = JsonSerializer.Deserialize<McpRequest>(e.Data);
            if (request != null)
            {
                var response = new McpResponse { id = request.id };
                Send(JsonSerializer.Serialize(response));
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to process packet: {ex.Message}");
        }
    }
}

public class McpRequest
{
    public string type { get; set; }
    public string id { get; set; }
    public JsonElement parameters { get; set; }
}

public class McpResponse
{
    public string id { get; set; }
    public object result { get; set; } = new { status = "ok" };
}

public static class Program
{
    public static void Main(string[] args)
    {
        if (!int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
            port = 8001;

        var server = new WebSocketServer(port);
        server.AddWebSocketService<McpBehavior>("/");
        server.Start();
        Console.WriteLine($"Godot MCP server listening on port {port}");
        Console.WriteLine("Press Enter to stop the server...");
        Console.ReadLine();
        server.Stop();
    }
}
