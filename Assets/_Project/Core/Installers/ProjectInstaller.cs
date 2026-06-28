using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        UnityEngine.Debug.Log("ProjectInstaller");
        Container.Bind<GameStateMachine>()
            .AsSingle();

        Container.Bind<ISceneLoader>()
            .To<SceneLoader>()
            .AsSingle();

        PhaseInstaller.Install(Container);

        Container.BindInterfacesAndSelfTo<BootstrapStartup>()
            .AsSingle()
            .NonLazy();

        // Container.Bind<StartupPhaseRegistry>()
        //     .AsSingle();
    }
}
