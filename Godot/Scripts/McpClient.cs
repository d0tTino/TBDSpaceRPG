using Godot;
using System.Text.Json;
using System.Threading.Tasks;

public partial class McpClient : Control
{
    [Export] public string Endpoint = "http://localhost:8001/mcp";

    public override void _Ready()
    {
        if (ProjectSettings.HasSetting("application/mcp_endpoint"))
        {
            var cfg = ProjectSettings.GetSetting("application/mcp_endpoint");
            if (cfg is StringName sn)
                Endpoint = sn.ToString();
            else if (cfg is string str)
                Endpoint = str;
        }

        GetNode<Button>("VBox/DockButton").Pressed += () => _ = SendCommand("Dock Ship");
        GetNode<Button>("VBox/UpgradeButton").Pressed += () => _ = SendCommand("Ship/Upgrade");
        GetNode<Button>("VBox/RelaunchButton").Pressed += () => _ = SendCommand("Ship/Relaunch");
    }

    public async Task SendCommand(string menuPath)
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
