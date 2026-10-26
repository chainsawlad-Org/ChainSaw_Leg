using UnityEngine.SceneManagement;

namespace ChainSawLeg.Infrastructure
{
    public sealed class UnityActiveSceneProvider : IActiveSceneProvider
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;
    }
}
