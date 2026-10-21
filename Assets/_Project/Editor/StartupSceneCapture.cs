#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class StartupSceneCapture
{
    public const string SessionKey = "ChainSawLeg.StartupScene";

    static StartupSceneCapture()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        string sceneName = EditorSceneManager.GetActiveScene().name;

        SessionState.SetString(SessionKey, sceneName);

        UnityEngine.Debug.Log(
            $"[StartupSceneCapture] Startup scene: {sceneName}");
    }
}

#endif