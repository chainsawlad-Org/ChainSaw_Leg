using Zenject;

public class SafeSceneContext : SceneContext
{
    protected override void OnDestroy()
    {
        if (Container != null)
            Container.UnbindAll();
    }
}
