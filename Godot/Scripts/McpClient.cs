using Godot;
using System.Text.Json;
using System.Threading.Tasks;

public partial class McpClient : Control
{
    private const string Endpoint = "http://localhost:8001/mcp";

    public override void _Ready()
    {
        GetNode<Button>("VBox/DockButton").Pressed += () => _ = SendCommand("Dock Ship");
        GetNode<Button>("VBox/UpgradeButton").Pressed += () => _ = SendCommand("Ship/Upgrade");
        GetNode<Button>("VBox/RelaunchButton").Pressed += () => _ = SendCommand("Ship/Relaunch");
    }

    private async Task SendCommand(string menuPath)
    {
        var payload = new
        {
            method = "execute_menu_item",
            @params = new { menuPath },
            id = System.Guid.NewGuid().ToString()
        };
        var json = JsonSerializer.Serialize(payload);
        var req = new HttpRequest();
        AddChild(req);
        var err = req.Request(Endpoint, new string[] { "Content-Type: application/json" }, HttpClient.Method.Post, json);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Request failed: {err}");
            req.QueueFree();
            return;
        }
        await ToSignal(req, HttpRequest.SignalName.RequestCompleted);
        req.QueueFree();
    }
}
