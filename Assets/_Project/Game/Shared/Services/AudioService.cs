using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global, static access point for AudioClips stored in an AudioLibrary
/// that lives in a Resources folder. Safe to call from anywhere, any scene.
/// </summary>
public static class AudioService
{
    // Must match: Assets/Resources/.../AudioLibrary.asset  (no extension)
    private const string LibraryResourcePath = "AudioLibrary";

    private static AudioLibrary library;
    private static Dictionary<AudioId, AudioClip> enumCache;

    private static Dictionary<AudioSource, float> audioSources;
    private const int audioSourcesCount = 5;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        library = Resources.Load<AudioLibrary>(LibraryResourcePath);
        if (library == null)
            Debug.LogError($"[AudioService] AudioLibrary not found at Resources/{LibraryResourcePath}.asset");

        audioSources = new Dictionary<AudioSource, float>(audioSourcesCount);
        
        GameObject audioObject = new GameObject("AudioObject");
        Object.DontDestroyOnLoad(audioObject);
        
        for (int i = 0; i < audioSourcesCount; i++)
        {
            AudioSource source = audioObject.AddComponent<AudioSource>();
            audioSources[source] = 0f;
            source.playOnAwake = false;
            source.loop = false;
            source.outputAudioMixerGroup = library.mixerGroup;
        }
    }

    public static AudioSource Play(AudioId id, float volume = 1f, float pitch = 1f)
    {
        AudioSource source = GetNextAudioSource();
        source.clip = GetClip(id);
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
        audioSources[source] = Time.time;
        return source;
    }

    /// <summary>Returns the clip for an id, or null (logs a warning).</summary>
    public static AudioClip GetClip(AudioId id)
    {
        if (TryGetClip(id, out var clip)) return clip;
        Debug.LogWarning($"[AudioService] No AudioClip for AudioId.{id}");
        return null;
    }

    /// <summary>Non-logging lookup.</summary>
    public static bool TryGetClip(AudioId id, out AudioClip clip)
    {
        clip = null;
        if (library == null) return false;

        EnsureCache();
        return enumCache.TryGetValue(id, out clip) && clip != null;
    }

    private static AudioSource GetNextAudioSource()
    {
        foreach (var audioSource in audioSources.Keys)
        {
            if (!audioSource.isPlaying) return audioSource;
        }
        
        AudioSource olderSource = null;
        float olderPlayTime = float.MaxValue;
        
        foreach (var audioSource in audioSources)
        {
            if (audioSource.Value < olderPlayTime)
            {
                olderSource = audioSource.Key;
                olderPlayTime = audioSource.Value;
            }
        }

        return olderSource;
    }
    
    private static void EnsureCache()
    {
        if (enumCache != null) return;

        enumCache = new Dictionary<AudioId, AudioClip>();
        if (library == null || library.clips == null) return;

        foreach (var clip in library.clips)
        {
            if (clip == null) continue;
            if (System.Enum.TryParse(clip.name, out AudioId id))
                enumCache[id] = clip;
        }
    }
}