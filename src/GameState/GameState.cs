using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public struct Vector3Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public class GameState
{
    [JsonPropertyName("upgradeLevel")]
    public int UpgradeLevel { get; set; }

    [JsonPropertyName("shipPosition")]
    public Vector3Data ShipPosition { get; set; }

    [JsonPropertyName("crewStats")]
    public CrewStats CrewStats { get; set; } = new CrewStats();

    public static GameState Load(string path)
    {
        if (!File.Exists(path))
            return new GameState();
        try
        {
            var json = File.ReadAllText(path);
            var state = JsonSerializer.Deserialize<GameState>(json);
            return state ?? new GameState();
        }
        catch
        {
            return new GameState();
        }
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(this);
        File.WriteAllText(path, json);
    }
}

public class CrewStats
{
    public int crewCount { get; set; }
    public int morale { get; set; }
}
