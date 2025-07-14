using Godot;
using System.IO;
using System.Text.Json;

public partial class StationDock : Control
{
    [Export] public string SaveFile = "Gameplay_Data/game_state.json";
    [Export] public int UpgradeLevel = 0;

    private McpClient _client;

    public override void _Ready()
    {
        _client = GetTree().Root.FindChild("HUD", true, false) as McpClient;
        GetNode<Button>("VBox/DockButton").Pressed += Dock;
        GetNode<Button>("VBox/UpgradeButton").Pressed += ApplyUpgrade;
        LoadState();
    }

    private async void Dock()
    {
        if (_client != null)
            await _client.SendCommand("Dock Ship");
        SaveState();
    }

    private async void ApplyUpgrade()
    {
        UpgradeLevel++;
        if (_client != null)
            await _client.SendCommand("Ship/Upgrade");
        SaveState();
    }

    private void SaveState()
    {
        Directory.CreateDirectory("Gameplay_Data");
        var json = JsonSerializer.Serialize(new { upgradeLevel = UpgradeLevel });
        File.WriteAllText(SaveFile, json);
    }

    private void LoadState()
    {
        if (!File.Exists(SaveFile))
            return;
        try
        {
            var json = File.ReadAllText(SaveFile);
            var state = JsonSerializer.Deserialize<UpgradeState>(json);
            if (state != null)
                UpgradeLevel = state.upgradeLevel;
        }
        catch { }
    }

    private class UpgradeState
    {
        public int upgradeLevel { get; set; }
    }
}
