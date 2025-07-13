using System;
using System.IO;

namespace TBDSpaceGame.Exporters
{
    public static class ScriptableObjectJsonExporter
    {
        public static void Run(string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            string filePath = Path.Combine(outputDir, "sample-data.json");
            string content = "{ \"name\": \"Example\", \"value\": 42 }";
            File.WriteAllText(filePath, content);
            Console.WriteLine($"Exported JSON to {filePath}");
        }
    }
}
