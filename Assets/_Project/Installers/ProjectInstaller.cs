using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<GameStateMachine>()
            .AsSingle();

        Container.Bind<ISceneLoader>()
            .To<SceneLoader>()
            .AsSingle();
    }
}
