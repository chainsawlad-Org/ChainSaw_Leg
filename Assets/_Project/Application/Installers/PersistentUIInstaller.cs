using UnityEngine;
using Zenject;

public class PersistentUIInstaller : MonoInstaller
{
    [SerializeField] private Transform uiRoot;

    public override void InstallBindings()
    {
        PauseMenuView pauseMenuView = PauseMenuViewFactory.BuildPauseMenuView(
            uiRoot,
            out SaveBrowserView saveBrowserView);

        Container.Bind<PauseMenuView>()
            .FromInstance(pauseMenuView)
            .AsSingle();

        Container.Bind<SaveBrowserView>()
            .FromInstance(saveBrowserView)
            .AsSingle();

        Container.BindInterfacesTo<PauseMenuCoordinator>()
            .AsSingle()
            .NonLazy();

        CheckpointSaveMenuView checkpointSaveMenuView =
            CheckpointSaveMenuViewFactory.BuildCheckpointSaveMenuView(uiRoot);

        Container.Bind<CheckpointSaveMenuView>()
            .FromInstance(checkpointSaveMenuView)
            .AsSingle();

        Container.BindInterfacesTo<CheckpointSaveMenuCoordinator>()
            .AsSingle()
            .NonLazy();
    }
}
