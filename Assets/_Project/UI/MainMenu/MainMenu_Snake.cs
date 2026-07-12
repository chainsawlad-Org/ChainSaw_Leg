using UnityEngine;

public class MainMenu_Snake : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Start Snake");
    }

    public void EndGame()
    {
        Debug.Log("Snake ended");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
