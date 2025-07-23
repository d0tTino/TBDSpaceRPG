using Godot;
using System.Text.Json;
using System.Threading.Tasks;

public partial class McpClient : Control
{
    [Export] public string Endpoint = "http://localhost:8002/mcp";

    public override void _Ready()
    {
        if (ProjectSettings.HasSetting("application/mcp_endpoint"))
        {
            var cfg = ProjectSettings.GetSetting("application/mcp_endpoint");
            if (cfg.VariantType == Variant.Type.StringName)
                Endpoint = ((StringName)cfg).ToString();
            else if (cfg.VariantType == Variant.Type.String)
                Endpoint = (string)cfg;
        }

        GetNode<Button>("VBox/DockButton").Pressed += () => _ = SendCommand("Dock Ship");
        GetNode<Button>("VBox/UpgradeButton").Pressed += () => _ = SendCommand("Ship/Upgrade");
        GetNode<Button>("VBox/RelaunchButton").Pressed += () => _ = SendCommand("Ship/Relaunch");
        var label = GetNode<Label>("VBox/UpgradeLabel");
        var toggle = GetNode<CheckButton>("VBox/ShowUpgradesToggle");
        toggle.Toggled += (toggled) => label.Visible = toggled;
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
