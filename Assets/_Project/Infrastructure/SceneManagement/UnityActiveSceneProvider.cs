using UnityEngine.SceneManagement;

public sealed class UnityActiveSceneProvider : IActiveSceneProvider
{
    public string ActiveSceneName => SceneManager.GetActiveScene().name;
}
