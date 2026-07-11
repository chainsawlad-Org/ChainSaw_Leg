using Zenject;

public class ServiceInstaller : Installer<ServiceInstaller>
{
    public override void InstallBindings()
    {
        AutoBinder.BindDerivedTypes<SceneService>(Container);
        Container.Bind<ITimeScaleController>()
            .To<UnityTimeScaleController>()
            .AsSingle();
        Container.Bind<GamePauseService>()
            .AsSingle();
        Container.Bind<GameplayInputBlockService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<InputService>()
            .AsSingle();
        Container.Bind<PauseMenuService>()
            .AsSingle();
        Container.Bind<PauseMenuExitCommandService>()
            .AsSingle();
    }
}
