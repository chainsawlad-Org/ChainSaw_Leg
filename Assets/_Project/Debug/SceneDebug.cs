using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("========== LOADED SCENES ==========");

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            Debug.Log(
                $"Scene: {scene.name} | " +
                $"Loaded: {scene.isLoaded} | " +
                $"Active: {scene == SceneManager.GetActiveScene()}");
        }

        Debug.Log("===================================");
    }
}