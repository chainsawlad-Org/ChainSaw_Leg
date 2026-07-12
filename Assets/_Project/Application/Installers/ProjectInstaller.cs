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

        Container.Bind<IPhaseFactory>()
            .To<PhaseFactory>()
            .AsSingle();

        PhaseInstaller.Install(Container);
        ServiceInstaller.Install(Container);

        Container.Bind<IBootstrapRunner>()
            .To<BootstrapRunner>()
            .AsSingle();

        Container.Bind<StartupPhaseRegistry>()
            .AsSingle();

        Container.Bind<IStartupResolver>()
            .To<StartupResolver>()
            .AsSingle();

        Container.BindInterfacesAndSelfTo<StartupRegistryInstaller>()
            .AsSingle()
            .NonLazy();

        Container.BindInterfacesAndSelfTo<BootstrapStartup>()
            .AsSingle()
            .NonLazy();
    }
}
