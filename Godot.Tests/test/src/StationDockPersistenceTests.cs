using Godot;
using Chickensoft.GoDotTest;
using Shouldly;
using System.IO;

public class StationDockPersistenceTests : TestClass
{
    public StationDockPersistenceTests(Node scene) : base(scene) { }

    [Test]
    public void SaveAndLoadUpgradeLevel()
    {
        var dock = new StationDock();
        dock.SaveFile = "test_game_state.json";
        dock.UpgradeFile = "test_upgrades.json";
        dock.UpgradeLevel = 5;
        dock.SaveUpgrade();
        dock.UpgradeLevel = 0;
        dock.LoadUpgrade();
        dock.UpgradeLevel.ShouldBe(5);
        File.Delete("test_upgrades.json");
        File.Delete("test_game_state.json");
    }
}
