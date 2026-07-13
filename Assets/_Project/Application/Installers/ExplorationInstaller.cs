using ChainSawLeg.Features.Exploration.Save;
using Zenject;

public sealed class ExplorationInstaller : Installer<ExplorationInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<ExplorationService>().AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationPlayerRegistry>().AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationSaveContextService>().AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationSaveContributor>().AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationSaveRestorer>().AsSingle();
        Container.Bind<IExplorationSceneTransitionService>()
            .To<ExplorationSceneTransitionService>()
            .AsSingle();
        Container.Bind<ExplorationGameSaveLoadService>().AsSingle();
        Container.Bind<ExplorationSaveCatalogService>().AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationCheckpointSaveService>().AsSingle();
        Container.Bind<CheckpointSaveRequestBroker>().AsSingle();
    }
}
