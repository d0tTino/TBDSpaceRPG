using Godot;
using System;
using System.Text.Json;
using System.Text;

public partial class McpGodotServer : Node
{
    private WebSocketServer _server = new();

    public override void _Ready()
    {
        var portEnv = Environment.GetEnvironmentVariable("PORT");
        if (!int.TryParse(portEnv, out var port))
            port = 8001;

        _server.Listen(port);
        GD.Print($"Godot MCP server listening on port {port}");
        _server.ClientConnected += id => GD.Print($"Client connected: {id}");
    }

    public override void _Process(double delta)
    {
        _server.Poll();

        foreach (var id in _server.GetPeerIds())
        {
            var peer = _server.GetPeer(id);
            while (peer.GetAvailablePacketCount() > 0)
            {
                var bytes = peer.GetPacket();
                var msg = Encoding.UTF8.GetString(bytes);
                try
                {
                    var request = JsonSerializer.Deserialize<McpRequest>(msg);
                    if (request != null)
                    {
                        var response = new McpResponse { id = request.id };
                        var respBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                        peer.PutPacket(respBytes);
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Failed to process packet: {e.Message}");
                }
            }
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
        using var gd = new Godot.Godot();
        gd.Init();
        var tree = new SceneTree();
        var server = new McpGodotServer();
        tree.Root.AddChild(server);
        gd.Run(tree);
        gd.Shutdown();
    }
}
