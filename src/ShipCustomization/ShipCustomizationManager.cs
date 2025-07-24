using Godot;

public partial class ShipCustomizationManager : Node
{
    [Export] public int ThrustLevel = 1;
    [Export] public int SpeedLevel = 1;
    [Export] public int RotationLevel = 1;
    [Export] public int BrakingLevel = 1;

    [Export] public SpaceshipMovement? Movement;

    public override void _Ready()
    {
        AddToGroup("ShipCustomizationManager");
        ApplyUpgrades();
    }

    public void UpgradeThrust()
    {
        ThrustLevel++;
        ApplyUpgrades();
    }

    public void UpgradeSpeed()
    {
        SpeedLevel++;
        ApplyUpgrades();
    }

    public void UpgradeRotation()
    {
        RotationLevel++;
        ApplyUpgrades();
    }

    public void UpgradeBraking()
    {
        BrakingLevel++;
        ApplyUpgrades();
    }

    public void ResetUpgrades()
    {
        ThrustLevel = SpeedLevel = RotationLevel = BrakingLevel = 1;
        ApplyUpgrades();
    }

    public void LogCurrentUpgrades()
    {
        GD.Print($"Thrust {ThrustLevel}, Speed {SpeedLevel}, Rotation {RotationLevel}, Braking {BrakingLevel}");
    }

    private void ApplyUpgrades()
    {
        if (Movement == null) return;
        Movement.ThrustForce = 10f * ThrustLevel;
        Movement.MaxSpeed = 20f * SpeedLevel;
        Movement.RotationSpeed = 90f * RotationLevel;
        Movement.BrakingForce = 5f * BrakingLevel;
    }
}
