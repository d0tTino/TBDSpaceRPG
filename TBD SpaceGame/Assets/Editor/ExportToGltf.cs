using System;
using System.IO;

namespace TBDSpaceGame.Exporters
{
    public static class ExportToGltf
    {
        public static void Run(string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            string filePath = Path.Combine(outputDir, "sample-export.gltf");
            string content = "{ \"asset\": { \"version\": \"2.0\" } }";
            File.WriteAllText(filePath, content);
            Console.WriteLine($"Exported glTF to {filePath}");
        }
    }
}
