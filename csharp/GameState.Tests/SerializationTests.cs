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
            upgradeLevel = 3,
            shipPosition = new Vector3Data { X = 1, Y = 2, Z = 3 },
            crewStats = new CrewStats { crewCount = 5, morale = 80 }
        };

        state.Save(SavePath);
        Assert.IsTrue(File.Exists(SavePath));

        var loaded = GameState.Load(SavePath);
        Assert.AreEqual(state.upgradeLevel, loaded.upgradeLevel);
        Assert.AreEqual(state.shipPosition.X, loaded.shipPosition.X);
        Assert.AreEqual(state.shipPosition.Y, loaded.shipPosition.Y);
        Assert.AreEqual(state.shipPosition.Z, loaded.shipPosition.Z);
        Assert.AreEqual(state.crewStats.crewCount, loaded.crewStats.crewCount);
        Assert.AreEqual(state.crewStats.morale, loaded.crewStats.morale);
    }

    [Test]
    public void Load_ReturnsDefaultState_WhenFileMissing()
    {
        var loaded = GameState.Load(SavePath);
        Assert.AreEqual(0, loaded.upgradeLevel);
        Assert.AreEqual(0, loaded.shipPosition.X);
        Assert.AreEqual(0, loaded.shipPosition.Y);
        Assert.AreEqual(0, loaded.shipPosition.Z);
        Assert.AreEqual(0, loaded.crewStats.crewCount);
        Assert.AreEqual(0, loaded.crewStats.morale);
    }

    [Test]
    public void Load_ReturnsDefaultState_OnInvalidJson()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
        File.WriteAllText(SavePath, "not a valid json");

        var loaded = GameState.Load(SavePath);
        Assert.AreEqual(0, loaded.upgradeLevel);
        Assert.AreEqual(0, loaded.shipPosition.X);
        Assert.AreEqual(0, loaded.shipPosition.Y);
        Assert.AreEqual(0, loaded.shipPosition.Z);
        Assert.AreEqual(0, loaded.crewStats.crewCount);
        Assert.AreEqual(0, loaded.crewStats.morale);
    }
}
