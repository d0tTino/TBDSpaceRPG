using System;
using System.IO;

namespace TBDSpaceGame.Exporters
{
    class Program
    {
        static void Main(string[] args)
        {
            string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
            string gltfDir = Path.Combine(root, "Assets_glTF");
            string jsonDir = Path.Combine(root, "Gameplay_Data");
            Directory.CreateDirectory(gltfDir);
            Directory.CreateDirectory(jsonDir);

            ExportToGltf.Run(gltfDir);
            ScriptableObjectJsonExporter.Run(jsonDir);
        }
    }
}
