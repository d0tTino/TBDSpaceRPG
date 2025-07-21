using Godot;

public partial class StationDock : Control
{
    [Export] public string SaveFile = "Gameplay_Data/game_state.json";
    [Export] public int UpgradeLevel = 0;
    [Export] public Vector3 ShipPosition = Vector3.Zero;
    [Export] public CrewStats CrewStats = new CrewStats();

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
        var state = new GameState
        {
            upgradeLevel = UpgradeLevel,
            shipPosition = new Vector3Data { X = ShipPosition.X, Y = ShipPosition.Y, Z = ShipPosition.Z },
            crewStats = CrewStats
        };
        state.Save(SaveFile);
    }

    private void LoadState()
    {
        var state = GameState.Load(SaveFile);
        UpgradeLevel = state.upgradeLevel;
        ShipPosition = new Vector3(state.shipPosition.X, state.shipPosition.Y, state.shipPosition.Z);
        CrewStats = state.crewStats;
    }
}
