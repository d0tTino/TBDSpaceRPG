using NUnit.Framework;
using Godot;

public class ShipCustomizationManagerTests
{
    [Test]
    public void UpgradeThrust_IncreasesLevelAndAppliesToMovement()
    {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeThrust();
        Assert.AreEqual(2, manager.ThrustLevel);
        Assert.AreEqual(20f, movement.ThrustForce);
    }

    [Test]
    public void ResetUpgrades_SetsLevelsToOne()
    {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeSpeed();
        manager.ResetUpgrades();
        Assert.AreEqual(1, manager.SpeedLevel);
        Assert.AreEqual(20f, movement.MaxSpeed);
    }

    [Test]
    public void UpgradeRotation_IncreasesLevelAndAppliesToMovement()
    {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeRotation();
        Assert.AreEqual(2, manager.RotationLevel);
        Assert.AreEqual(180f, movement.RotationSpeed);
    }

    [Test]
    public void UpgradeBraking_IncreasesLevelAndAppliesToMovement()
    {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeBraking();
        Assert.AreEqual(2, manager.BrakingLevel);
        Assert.AreEqual(10f, movement.BrakingForce);
    }
}
