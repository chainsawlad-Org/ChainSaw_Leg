using ChainSawLeg.Core.SaveSystem;
using UnityEngine;
using Zenject;

public class PersistentUIInstaller : MonoInstaller
{
    [SerializeField] private Transform uiRoot;

    public override void InstallBindings()
    {
        Container.Bind<PersistentUIViewRegistry>()
            .AsSingle()
            .WithArguments(uiRoot, GameSaveSlotCatalog.CheckpointSlotIds.Count)
            .NonLazy();

        Container.Bind<PauseMenuView>()
            .FromResolveGetter<PersistentUIViewRegistry>(registry => registry.PauseMenuView)
            .AsSingle();

        Container.Bind<SaveBrowserView>()
            .FromResolveGetter<PersistentUIViewRegistry>(registry => registry.SaveBrowserView)
            .AsSingle();

        Container.BindInterfacesTo<PauseMenuCoordinator>()
            .AsSingle()
            .NonLazy();

        Container.BindInterfacesTo<MainMenuSaveBrowserCoordinator>()
            .AsSingle()
            .NonLazy();

        Container.Bind<CheckpointSaveMenuView>()
            .FromResolveGetter<PersistentUIViewRegistry>(registry => registry.CheckpointSaveMenuView)
            .AsSingle();

        Container.BindInterfacesTo<CheckpointSaveMenuCoordinator>()
            .AsSingle()
            .NonLazy();
    }
}
