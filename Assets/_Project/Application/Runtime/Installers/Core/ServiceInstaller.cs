using ChainSawLeg.Core.SaveSystem;
using Zenject;

public class ServiceInstaller : Installer<ServiceInstaller>
{
    public override void InstallBindings()
    {
        AutoBinder.BindDerivedTypes<ApplicationServiceBase>(Container);
        AutoBinder.BindAssignableTo<IApplicationService>(Container);

        Container.Bind<ITimeScaleController>()
            .To<UnityTimeScaleController>()
            .AsSingle();
        Container.Bind<GamePauseService>()
            .AsSingle();
        Container.Bind<IGameplayInputBlockService>()
            .To<GameplayInputBlockService>()
            .AsSingle();
        Container.Bind<IRuntimeErrorLogger>()
            .To<UnityRuntimeErrorLogger>()
            .AsSingle();
        Container.Bind<IApplicationQuitService>()
            .To<UnityApplicationQuitService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<InputService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<PauseMenuService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<PauseMenuExitCommandService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<MainMenuStartCommandService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<BattleSceneTransitionService>()
            .AsSingle();
        Container.Bind<BattleSessionService>()
            .AsSingle();
        Container.BindInterfacesAndSelfTo<MainMenuSaveBrowserRequestBroker>()
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
    }
}
