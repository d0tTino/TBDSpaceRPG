using Godot;

public partial class StationDock : Control
{
    [Export] public string SaveFile = "Gameplay_Data/game_state.json";
    [Export] public string UpgradeFile = "Gameplay_Data/upgrades.json";
    [Export] public int UpgradeLevel = 0;
    [Export] public Vector3 ShipPosition = Vector3.Zero;
    public CrewStats CrewStats = new CrewStats();

    private McpClient _client;
    private Label? _upgradeLabel;

    public override void _Ready()
    {
        _client = GetTree().Root.FindChild("HUD", true, false) as McpClient;
        _upgradeLabel = GetTree().Root.FindChild("UpgradeLabel", true, false) as Label;
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
        UpdateUpgradeLabel();
    }

    private void SaveState()
    {
        var state = new GameState
        {
            upgradeLevel = UpgradeLevel,
            shipPosition = new Vector3Data { X = ShipPosition.X, Y = ShipPosition.Y, Z = ShipPosition.Z },
            crewStats = CrewStats
        };
        state.Save(SaveFile);
        SaveUpgrade();
    }

    private void LoadState()
    {
        var state = GameState.Load(SaveFile);
        UpgradeLevel = state.upgradeLevel;
        ShipPosition = new Vector3(state.shipPosition.X, state.shipPosition.Y, state.shipPosition.Z);
        CrewStats = state.crewStats;
        LoadUpgrade();
        UpdateUpgradeLabel();
    }

    public void SaveUpgrade()
    {
        var data = new { upgradeLevel = UpgradeLevel };
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(UpgradeFile)!);
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        System.IO.File.WriteAllText(UpgradeFile, json);
    }

    public void LoadUpgrade()
    {
        if (!System.IO.File.Exists(UpgradeFile)) return;
        try
        {
            var json = System.IO.File.ReadAllText(UpgradeFile);
            var data = System.Text.Json.JsonSerializer.Deserialize<UpgradeData>(json);
            if (data != null) UpgradeLevel = data.upgradeLevel;
        }
        catch { }
    }

    private void UpdateUpgradeLabel()
    {
        if (_upgradeLabel != null)
            _upgradeLabel.Text = $"Upgrade Level: {UpgradeLevel}";
    }

    private class UpgradeData
    {
        public int upgradeLevel { get; set; }
    }
}
