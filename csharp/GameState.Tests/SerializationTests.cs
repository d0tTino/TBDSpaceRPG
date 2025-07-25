using System.IO;

using NUnit.Framework;

public class SerializationTests
{
    private const string SavePath = "Gameplay_Data/test_state.json";

    [TearDown]
    public void Cleanup()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    [Test]
    public void SaveAndLoad_PreservesData()
    {
        var state = new GameState
        {
            UpgradeLevel = 3,
            ShipPosition = new Vector3Data { X = 1, Y = 2, Z = 3 },
            CrewStats = new CrewStats { crewCount = 5, morale = 80 }
        };

        state.Save(SavePath);
        Assert.IsTrue(File.Exists(SavePath));

        var loaded = GameState.Load(SavePath);
        Assert.AreEqual(state.UpgradeLevel, loaded.UpgradeLevel);
        Assert.AreEqual(state.ShipPosition.X, loaded.ShipPosition.X);
        Assert.AreEqual(state.ShipPosition.Y, loaded.ShipPosition.Y);
        Assert.AreEqual(state.ShipPosition.Z, loaded.ShipPosition.Z);
        Assert.AreEqual(state.CrewStats.crewCount, loaded.CrewStats.crewCount);
        Assert.AreEqual(state.CrewStats.morale, loaded.CrewStats.morale);
    }

    [Test]
    public void Load_ReturnsDefaultState_WhenFileMissing()
    {
        var loaded = GameState.Load(SavePath);
        Assert.AreEqual(0, loaded.UpgradeLevel);
        Assert.AreEqual(0, loaded.ShipPosition.X);
        Assert.AreEqual(0, loaded.ShipPosition.Y);
        Assert.AreEqual(0, loaded.ShipPosition.Z);
        Assert.AreEqual(0, loaded.CrewStats.crewCount);
        Assert.AreEqual(0, loaded.CrewStats.morale);
    }

    [Test]
    public void Load_ReturnsDefaultState_OnInvalidJson()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
        File.WriteAllText(SavePath, "not a valid json");

        var loaded = GameState.Load(SavePath);
        Assert.AreEqual(0, loaded.UpgradeLevel);
        Assert.AreEqual(0, loaded.ShipPosition.X);
        Assert.AreEqual(0, loaded.ShipPosition.Y);
        Assert.AreEqual(0, loaded.ShipPosition.Z);
        Assert.AreEqual(0, loaded.CrewStats.crewCount);
        Assert.AreEqual(0, loaded.CrewStats.morale);
    }
}
