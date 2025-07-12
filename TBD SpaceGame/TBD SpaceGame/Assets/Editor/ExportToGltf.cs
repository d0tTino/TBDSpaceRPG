using System.IO;
using UnityEditor;
using UnityEditor.Formats.GLTF;
using UnityEngine;

public static class ExportToGltf
{
    [MenuItem("Tools/Export All To glTF")]
    public static void ExportAll()
    {
        string targetDir = Path.Combine(Application.dataPath, "..", "..", "Assets_glTF");
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            string sceneName = Path.GetFileNameWithoutExtension(scene.path);
            string path = Path.Combine(targetDir, sceneName + ".gltf");
            var exporter = new GLTFSceneExporter(new[] { scene.path });
            exporter.SaveGLTF(path);
        }

        string[] prefabs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabs)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;
            string fileName = Path.GetFileNameWithoutExtension(prefabPath) + ".gltf";
            string path = Path.Combine(targetDir, fileName);
            var exporter = new GLTFSceneExporter(new[] { prefab });
            exporter.SaveGLTF(path);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Exported glTF files to {targetDir}");
    }
}
