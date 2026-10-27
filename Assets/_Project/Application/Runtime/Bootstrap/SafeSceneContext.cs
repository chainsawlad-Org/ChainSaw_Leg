using Zenject;

namespace ChainSawLeg.Application
{
    public class SafeSceneContext : SceneContext
    {
        protected override void OnDestroy()
        {
            if (Container != null)
                Container.UnbindAll();
        }
    }
}
