using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class ScriptableObjectJsonExporter
{
    [MenuItem("Tools/Export ScriptableObjects to JSON")]
    public static void ExportAll()
    {
        // Identify ScriptableObject types defined under Assets/Scripts
        var scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] {"Assets/Scripts"});
        var soTypes = new List<System.Type>();
        foreach (var sg in scriptGuids)
        {
            var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(sg));
            var type = mono?.GetClass();
            if (type != null && typeof(ScriptableObject).IsAssignableFrom(type) && !type.IsAbstract)
            {
                soTypes.Add(type);
            }
        }

        var dataDirectory = Path.Combine(Application.dataPath, "../Gameplay_Data");
        Directory.CreateDirectory(dataDirectory);
        int exported = 0;

        foreach (var type in soTypes)
        {
            var assetGuids = AssetDatabase.FindAssets($"t:{type.Name}");
            foreach (var ag in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(ag);
                var obj = AssetDatabase.LoadAssetAtPath(assetPath, type) as ScriptableObject;
                if (obj == null)
                    continue;
                var json = JsonUtility.ToJson(obj, true);
                var fileName = obj.name + ".json";
                File.WriteAllText(Path.Combine(dataDirectory, fileName), json);
                exported++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Exported {exported} ScriptableObjects to {dataDirectory}");
    }
}
