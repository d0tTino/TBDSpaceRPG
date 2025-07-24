using Godot;
using Chickensoft.GoDotTest;
using Shouldly;

public partial class ShipUpgradeMenuHelperTests : TestClass {
    private partial class TestHelper : Node {
        public void CreateTestShip() {
            var ship = new CharacterBody3D { Name = "TestShip" };
            var move = new SpaceshipMovement();
            var manager = new ShipCustomizationManager { Movement = move };
            manager.AddToGroup("ShipCustomizationManager");
            ship.AddChild(move);
            ship.AddChild(manager);
            AddChild(ship);
        }

        public ShipCustomizationManager? GetManager() {
            foreach (var node in GetTree().GetNodesInGroup("ShipCustomizationManager"))
                if (node is ShipCustomizationManager mgr)
                    return mgr;
            return null;
        }
    }

    public ShipUpgradeMenuHelperTests(Node scene) : base(scene) { }

    [Test]
    public void CreateTestShip_AddsManagerToGroup() {
        var helper = new TestHelper();
        TestScene.AddChild(helper);
        helper.CreateTestShip();

        if (helper.GetChildCount() > 0 && helper.GetChild(0) is Node ship) {
            helper.RemoveChild(ship);
            TestScene.AddChild(ship);
        }

        var manager = helper.GetManager();

        manager.ShouldNotBeNull();
        manager.ShouldBeOfType<ShipCustomizationManager>();
    }
}
