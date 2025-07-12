using NUnit.Framework;
using UnityEngine;

public class ShipCustomizationManagerTests
{
    [Test]
    public void UpgradeThrust_IncreasesLevelAndAppliesToMovement()
    {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { movement = movement };
        manager.UpgradeThrust();
        Assert.AreEqual(2, manager.thrustLevel);
        Assert.AreEqual(20f, movement.thrustForce);
    }

    [Test]
    public void ResetUpgrades_SetsLevelsToOne()
    {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { movement = movement };
        manager.UpgradeSpeed();
        manager.ResetUpgrades();
        Assert.AreEqual(1, manager.speedLevel);
        Assert.AreEqual(20f, movement.maxSpeed);
    }
}
