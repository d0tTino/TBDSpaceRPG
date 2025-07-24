using System.Reflection;
using Godot;
using Chickensoft.GoDotTest;
using Shouldly;

public class ShipUpgradeMenuHelperTests : TestClass {
    public ShipUpgradeMenuHelperTests(Node scene) : base(scene) { }

    [Test]
    public void CreateTestShipAddsManager() {
        var helper = new ShipUpgradeMenuHelper();
        TestScene.AddChild(helper);

        typeof(ShipUpgradeMenuHelper)
            .GetMethod("CreateTestShip", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(helper, null);

        var manager = typeof(ShipUpgradeMenuHelper)
            .GetMethod("GetManager", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(helper, null);

        manager.ShouldNotBeNull();
    }
}
