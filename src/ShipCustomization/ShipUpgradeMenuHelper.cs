#if TOOLS
using Godot;

[Tool]
public partial class ShipUpgradeMenuHelper : EditorPlugin
{
    public override void _EnterTree()
    {
        AddToolMenuItem("Ship/Create Test Ship", Callable.From(CreateTestShip));
        AddToolMenuItem("Ship/Upgrade Thrust", Callable.From(UpgradeThrust));
        AddToolMenuItem("Ship/Upgrade Speed", Callable.From(UpgradeSpeed));
        AddToolMenuItem("Ship/Upgrade Rotation", Callable.From(UpgradeRotation));
        AddToolMenuItem("Ship/Upgrade Braking", Callable.From(UpgradeBraking));
        AddToolMenuItem("Ship/Reset All Upgrades", Callable.From(ResetUpgrades));
        AddToolMenuItem("Ship/Log Current Upgrades", Callable.From(LogUpgrades));
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem("Ship/Create Test Ship");
        RemoveToolMenuItem("Ship/Upgrade Thrust");
        RemoveToolMenuItem("Ship/Upgrade Speed");
        RemoveToolMenuItem("Ship/Upgrade Rotation");
        RemoveToolMenuItem("Ship/Upgrade Braking");
        RemoveToolMenuItem("Ship/Reset All Upgrades");
        RemoveToolMenuItem("Ship/Log Current Upgrades");
    }

    private void CreateTestShip()
    {
        var ship = new CharacterBody3D { Name = "TestShip" };
        var move = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = move };
        manager.AddToGroup("ShipCustomizationManager");
        ship.AddChild(move);
        ship.AddChild(manager);
        AddChild(ship);
    }

    private void UpgradeThrust() => GetManager()?.UpgradeThrust();
    private void UpgradeSpeed() => GetManager()?.UpgradeSpeed();
    private void UpgradeRotation() => GetManager()?.UpgradeRotation();
    private void UpgradeBraking() => GetManager()?.UpgradeBraking();
    private void ResetUpgrades() => GetManager()?.ResetUpgrades();
    private void LogUpgrades() => GetManager()?.LogCurrentUpgrades();

    private ShipCustomizationManager? GetManager()
    {
        foreach (var node in GetTree().GetNodesInGroup("ShipCustomizationManager"))
            if (node is ShipCustomizationManager mgr)
                return mgr;
        return null;
    }
}
#endif
