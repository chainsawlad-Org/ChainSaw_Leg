using UnityEngine;

public class MainMenu_Snake : MonoBehaviour
{
    public void StartGame()
    {
    }

    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
