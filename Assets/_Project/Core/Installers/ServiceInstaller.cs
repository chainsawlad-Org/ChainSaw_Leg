using Zenject;

public class ServiceInstaller : Installer<ServiceInstaller>
{
    public override void InstallBindings()
    {
        AutoBinder.BindDerivedTypes<SceneService>(Container);
        Container.BindInterfacesAndSelfTo<InputService>()
            .AsSingle();
        Container.Bind<PauseMenuService>()
            .AsSingle();
    }
}
