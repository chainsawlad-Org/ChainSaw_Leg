using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<MainMenuUI>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.BindInterfacesTo<MainMenuCoordinator>()
            .AsSingle()
            .NonLazy();
    }
}
