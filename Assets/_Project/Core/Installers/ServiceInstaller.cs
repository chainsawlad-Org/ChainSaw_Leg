using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
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
        Container.Bind<IRuntimeErrorLogger>()
            .To<UnityRuntimeErrorLogger>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<InputService>()
            .AsSingle();
        Container.Bind<PauseMenuService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<PauseMenuExitCommandService>()
            .AsSingle();
        Container.Bind<IGameSaveSerializer>()
            .To<OdinGameSaveSerializer>()
            .AsSingle();
        Container.Bind<IGameSaveStorageProvider>()
            .To<FileGameSaveStorageProvider>()
            .AsSingle();
        Container.Bind<IGameSaveRuntimeMetadataProvider>()
            .To<UnityGameSaveRuntimeMetadataProvider>()
            .AsSingle();
        Container.Bind<GameSaveValidationService>()
            .AsSingle();
        Container.Bind<GameSaveMigrationService>()
            .AsSingle();
        Container.Bind<GameSaveCoordinator>()
            .AsSingle();
        Container.Bind<GameSavePendingRestoreService>()
            .AsSingle();
        Container.Bind<CheckpointGameSaveSlotRotationService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationPlayerRegistry>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationSaveContextService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationSaveContributor>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationSaveRestorer>()
            .AsSingle();
        Container.Bind<IExplorationSceneTransitionService>()
            .To<ExplorationSceneTransitionService>()
            .AsSingle();
        Container.Bind<ExplorationGameSaveLoadService>()
            .AsSingle();
        Container.Bind<ExplorationSaveCatalogService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<ExplorationCheckpointSaveService>()
            .AsSingle();
        Container.Bind<CheckpointSaveRequestBroker>()
            .AsSingle();
    }
}
