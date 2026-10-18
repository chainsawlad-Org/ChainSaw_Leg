using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "MyGame/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Folder to scan (relative to project, e.g. Assets/Audio/SFX)")]
    public string sourceFolder = "Assets/_Project/Content/Audio/SFX";

    public AudioMixerGroup mixerGroup;

    [Header("Auto-filled. Do not edit manually.")]
    public AudioClip[] clips;

    [HideInInspector] public string[] clipNames;
    
    private System.Collections.Generic.Dictionary<string, AudioClip> lookup;

    /// <summary>Get a clip by its generated enum id. Returns null if not found.</summary>
    public AudioClip Get(AudioId id)
    {
        EnsureLookup();
        return lookup.TryGetValue(id.ToString(), out var clip) ? clip : null;
    }

    /// <summary>Try-get variant.</summary>
    public bool TryGet(AudioId id, out AudioClip clip)
    {
        EnsureLookup();
        return lookup.TryGetValue(id.ToString(), out clip);
    }

    private void EnsureLookup()
    {
        if (lookup != null) return;
        lookup = new System.Collections.Generic.Dictionary<string, AudioClip>(clips.Length);
        foreach (var c in clips)
        {
            if (c == null) continue;
            if (lookup.ContainsKey(c.name))
                Debug.LogWarning($"[AudioLibrary] Duplicate clip name '{c.name}' — using first one.");
            else
                lookup[c.name] = c;
        }
    }

    private void OnValidate() => lookup = null;
}