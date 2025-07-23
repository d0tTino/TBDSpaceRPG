using Godot;
using Chickensoft.GoDotTest;
using Shouldly;

public class ShipCustomizationManagerTests : TestClass {
    public ShipCustomizationManagerTests(Node scene) : base(scene) { }

    [Test]
    public void UpgradeThrust_IncreasesLevelAndAppliesToMovement() {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeThrust();
        manager.ThrustLevel.ShouldBe(2);
        movement.ThrustForce.ShouldBe(20f);
    }

    [Test]
    public void ResetUpgrades_SetsLevelsToOne() {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeSpeed();
        manager.ResetUpgrades();
        manager.SpeedLevel.ShouldBe(1);
        movement.MaxSpeed.ShouldBe(20f);
    }

    [Test]
    public void UpgradeSpeed_IncreasesLevelAndAppliesToMovement() {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeSpeed();
        manager.SpeedLevel.ShouldBe(2);
        movement.MaxSpeed.ShouldBe(40f);
    }

    [Test]
    public void UpgradeRotation_IncreasesLevelAndAppliesToMovement() {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeRotation();
        manager.RotationLevel.ShouldBe(2);
        movement.RotationSpeed.ShouldBe(180f);
    }

    [Test]
    public void UpgradeBraking_IncreasesLevelAndAppliesToMovement() {
        var movement = new SpaceshipMovement();
        var manager = new ShipCustomizationManager { Movement = movement };
        manager.UpgradeBraking();
        manager.BrakingLevel.ShouldBe(2);
        movement.BrakingForce.ShouldBe(10f);
    }
}
