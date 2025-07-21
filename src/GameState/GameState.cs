using System.IO;
using System.Text.Json;

public struct Vector3Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public class GameState
{
    public int upgradeLevel { get; set; }
    public Vector3Data shipPosition { get; set; }
    public CrewStats crewStats { get; set; } = new CrewStats();

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
