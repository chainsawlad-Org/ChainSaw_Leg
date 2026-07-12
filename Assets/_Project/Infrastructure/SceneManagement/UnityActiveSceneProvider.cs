// Placement: Docs/Ru/02_ProjectStructure.md:192-202. Quote: "Содержит управление сценами."

using UnityEngine.SceneManagement;

public sealed class UnityActiveSceneProvider : IActiveSceneProvider
{
    public string ActiveSceneName => SceneManager.GetActiveScene().name;
}
