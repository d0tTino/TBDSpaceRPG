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
        var gamePath = Path.GetTempFileName();
        var upgradePath = Path.GetTempFileName();

        try {
            var dock = new StationDock();
            dock.SaveFile = gamePath;
            dock.UpgradeFile = upgradePath;
            dock.UpgradeLevel = 5;
            dock.SaveUpgrade();
            dock.UpgradeLevel = 0;
            dock.LoadUpgrade();
            dock.UpgradeLevel.ShouldBe(5);
        }
        finally {
            if (File.Exists(upgradePath)) File.Delete(upgradePath);
            if (File.Exists(gamePath)) File.Delete(gamePath);
        }
    }

    [Test]
    public void SaveState_WritesGameAndUpgradeFiles()
    {
        var gamePath = Path.GetTempFileName();
        var upgradePath = Path.GetTempFileName();

        try {
            var dock = new StationDock();
            dock.SaveFile = gamePath;
            dock.UpgradeFile = upgradePath;
            dock.UpgradeLevel = 2;
            dock.ShipPosition = new Vector3(1, 2, 3);
            dock.CrewStats = new CrewStats { crewCount = 4, morale = 7 };

            typeof(StationDock)
                .GetMethod("SaveState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(dock, null);

            var state = GameState.Load(gamePath);
            state.upgradeLevel.ShouldBe(2);
            state.shipPosition.X.ShouldBe(1);
            state.shipPosition.Y.ShouldBe(2);
            state.shipPosition.Z.ShouldBe(3);
            state.crewStats.crewCount.ShouldBe(4);
            state.crewStats.morale.ShouldBe(7);

            var upgradeJson = File.ReadAllText(upgradePath);
            upgradeJson.ShouldContain("\"upgradeLevel\":2");
        }
        finally {
            if (File.Exists(upgradePath)) File.Delete(upgradePath);
            if (File.Exists(gamePath)) File.Delete(gamePath);
        }
    }
}
