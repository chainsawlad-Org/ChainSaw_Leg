#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ChainSawLeg.Infrastructure;

public class AudioAssetsHandler : AssetPostprocessor
{
    private const string TargetFolder = "Assets/_Project/Content/Audio/SFX";

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool touchedAudio = importedAssets.Concat(deletedAssets).Concat(movedAssets)
            .Any(p => p.StartsWith(TargetFolder) &&
                      !p.EndsWith(".meta") &&
                      AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(AudioClip));

        if (!touchedAudio)
        {
            touchedAudio = deletedAssets.Concat(movedFromAssetPaths)
                .Any(p => p.StartsWith(TargetFolder) &&
                          p.EndsWith(".meta") == false &&
                          IsLikelyAudioExtension(p));
        }

        if (!touchedAudio) return;

        EditorApplication.delayCall += RefreshAllLibraries;
    }

    private static bool IsLikelyAudioExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".wav" || ext == ".mp3" || ext == ".ogg" ||
               ext == ".aif" || ext == ".aiff" || ext == ".flac";
    }

    private static void RefreshAllLibraries()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioLibrary");
        bool anyChanged = false;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var lib = AssetDatabase.LoadAssetAtPath<AudioLibrary>(path);
            if (lib == null) continue;
            if (string.IsNullOrEmpty(lib.sourceFolder)) continue;
            if (!lib.sourceFolder.StartsWith(TargetFolder)) continue;

            AudioLibraryEditor.Populate(lib);
            EditorUtility.SetDirty(lib);

            if (AudioIdGenerator.Regenerate(lib))
                anyChanged = true;
        }

        AssetDatabase.SaveAssets();
        if (anyChanged)
            AssetDatabase.Refresh();
    }
}
#endif
