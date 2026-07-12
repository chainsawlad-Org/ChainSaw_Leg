using System;
using System.Collections.Generic;
using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using Cysharp.Threading.Tasks;
using Zenject;

public sealed class CheckpointSaveMenuPresenter : IInitializable, IDisposable
{
    private readonly CheckpointSaveMenuView view;
    private readonly GameStateMachine gameStateMachine;
    private readonly ExplorationSaveCatalogService saveCatalogService;
    private readonly ExplorationCheckpointSaveService checkpointSaveService;
    private readonly CheckpointSaveRequestBroker saveRequestBroker;
    private readonly IRuntimeErrorLogger errorLogger;

    private UniTaskCompletionSource<bool> pendingResult;
    private string pendingCheckpointId;
    private bool isSaveInProgress;

    public CheckpointSaveMenuPresenter(
        CheckpointSaveMenuView view,
        GameStateMachine gameStateMachine,
        ExplorationSaveCatalogService saveCatalogService,
        ExplorationCheckpointSaveService checkpointSaveService,
        CheckpointSaveRequestBroker saveRequestBroker,
        IRuntimeErrorLogger errorLogger)
    {
        this.view = view;
        this.gameStateMachine = gameStateMachine;
        this.saveCatalogService = saveCatalogService;
        this.checkpointSaveService = checkpointSaveService;
        this.saveRequestBroker = saveRequestBroker;
        this.errorLogger = errorLogger;
    }

    public void Initialize()
    {
        view.SlotSaveRequested += OnSlotSaveRequested;
        view.BackRequested += OnBackRequested;
        gameStateMachine.StateChanged += OnStateChanged;
        saveRequestBroker.SaveRequested += RequestSaveAsync;
    }

    public void Dispose()
    {
        view.SlotSaveRequested -= OnSlotSaveRequested;
        view.BackRequested -= OnBackRequested;
        gameStateMachine.StateChanged -= OnStateChanged;
        saveRequestBroker.SaveRequested -= RequestSaveAsync;
    }

    public async UniTask<bool> RequestSaveAsync(string checkpointId, CancellationToken cancellationToken)
    {
        if (gameStateMachine.HasOverlayOfType<CheckpointSavePhase>())
            return false;

        pendingCheckpointId = checkpointId;
        pendingResult = new UniTaskCompletionSource<bool>();

        await gameStateMachine.PushOverlay<CheckpointSavePhase>();

        IReadOnlyList<GameSaveCatalogEntry> entries =
            await saveCatalogService.GetCheckpointSaveMenuEntriesAsync(cancellationToken);

        view.SetInteractionEnabled(true);
        view.ShowEntries(entries);
        view.Show();

        bool result;

        try
        {
            result = await pendingResult.Task.AttachExternalCancellation(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            result = false;
        }
        finally
        {
            pendingResult = null;
            pendingCheckpointId = null;
        }

        if (gameStateMachine.IsTopOverlay<CheckpointSavePhase>())
            await gameStateMachine.PopOverlay();

        view.Hide();

        return result;
    }

    private void OnSlotSaveRequested(string slotId)
    {
        if (pendingResult == null || isSaveInProgress)
            return;

        SaveToSlotAsync(slotId, pendingCheckpointId).Forget();
    }

    private async UniTask SaveToSlotAsync(string slotId, string checkpointId)
    {
        UniTaskCompletionSource<bool> resultSource = pendingResult;
        isSaveInProgress = true;
        view.SetInteractionEnabled(false);

        try
        {
            await checkpointSaveService.SaveCheckpointToSlotAsync(slotId, checkpointId, CancellationToken.None);
            resultSource.TrySetResult(true);
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, $"Checkpoint save failed: {slotId}");
        }
        finally
        {
            isSaveInProgress = false;

            if (pendingResult == resultSource)
                view.SetInteractionEnabled(true);
        }
    }

    private void OnBackRequested()
    {
        pendingResult?.TrySetResult(false);
    }

    private void OnStateChanged()
    {
        if (pendingResult == null)
            return;

        if (!gameStateMachine.IsTopOverlay<CheckpointSavePhase>())
            pendingResult.TrySetResult(false);
    }
}
