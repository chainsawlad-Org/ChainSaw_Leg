using ChainSawLeg.UI;
using Zenject;

namespace ChainSawLeg.Composition
{
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
}
