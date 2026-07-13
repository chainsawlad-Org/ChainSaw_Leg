using UnityEngine;

public sealed class PersistentUIViewRegistry
{
    public PersistentUIViewRegistry(Transform uiRoot, int checkpointSlotCount)
    {
        PauseMenuView = PauseMenuViewFactory.BuildPauseMenuView(
            uiRoot,
            out SaveBrowserView saveBrowserView);
        SaveBrowserView = saveBrowserView;
        CheckpointSaveMenuView = CheckpointSaveMenuViewFactory.BuildCheckpointSaveMenuView(
            uiRoot,
            checkpointSlotCount);
    }

    public PauseMenuView PauseMenuView { get; }
    public SaveBrowserView SaveBrowserView { get; }
    public CheckpointSaveMenuView CheckpointSaveMenuView { get; }
}
