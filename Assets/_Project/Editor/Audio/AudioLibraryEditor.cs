#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using ChainSawLeg.Infrastructure;

[CustomEditor(typeof(AudioLibrary))]
public class AudioLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var lib = (AudioLibrary)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Refresh From Folder"))
        {
            Populate(lib);
            AudioIdGenerator.Regenerate(lib);
            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
        }
    }

    public static void Populate(AudioLibrary lib)
    {
        if (string.IsNullOrEmpty(lib.sourceFolder) || !AssetDatabase.IsValidFolder(lib.sourceFolder))
        {
            Debug.LogWarning($"[AudioLibrary] Invalid folder: '{lib.sourceFolder}'");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { lib.sourceFolder });

        var clips = new List<AudioClip>(guids.Length);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
                clips.Add(clip);
        }

        clips.Sort((a, b) =>
            string.Compare(AssetDatabase.GetAssetPath(a),
                AssetDatabase.GetAssetPath(b),
                System.StringComparison.OrdinalIgnoreCase));

        lib.clips = clips.ToArray();
        lib.clipNames = clips.Select(c => c.name).ToArray();

        //Debug.Log($"[AudioLibrary] '{lib.name}' now has {lib.clips.Length} clip(s) from '{lib.sourceFolder}'");
    }
}
#endif
