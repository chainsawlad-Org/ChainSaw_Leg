using System;
using System.Collections.Generic;
using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using Cysharp.Threading.Tasks;
using Zenject;

public sealed class CheckpointSaveMenuCoordinator : IInitializable, IDisposable
{
    private readonly CheckpointSaveMenuView view;
    private readonly GameStateMachine gameStateMachine;
    private readonly ExplorationSaveCatalogService saveCatalogService;
    private readonly ExplorationCheckpointSaveService checkpointSaveService;
    private readonly CheckpointSaveRequestBroker saveRequestBroker;
    private readonly IRuntimeErrorLogger errorLogger;

    private UniTaskCompletionSource<bool> pendingResult;
    private CancellationTokenSource activeRequestCancellation;
    private string pendingCheckpointId;
    private bool isSaveInProgress;

    public CheckpointSaveMenuCoordinator(
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
        activeRequestCancellation?.Cancel();
        pendingResult?.TrySetResult(false);
    }

    public async UniTask<bool> RequestSaveAsync(string checkpointId, CancellationToken cancellationToken)
    {
        if (gameStateMachine.HasOverlayOfType<CheckpointSavePhase>())
            return false;

        var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var resultSource = new UniTaskCompletionSource<bool>();
        activeRequestCancellation = requestCancellation;
        pendingCheckpointId = checkpointId;
        pendingResult = resultSource;
        bool result = false;

        try
        {
            CancellationToken requestToken = requestCancellation.Token;
            await gameStateMachine.PushOverlay<CheckpointSavePhase>();
            requestToken.ThrowIfCancellationRequested();

            IReadOnlyList<GameSaveCatalogEntry> entries =
                await saveCatalogService.GetCheckpointSaveMenuEntriesAsync(requestToken);

            requestToken.ThrowIfCancellationRequested();
            view.SetInteractionEnabled(true);
            view.ShowEntries(SaveSlotViewDataMapper.Map(entries));
            view.Show();
            result = await resultSource.Task.AttachExternalCancellation(requestToken);
        }
        catch (OperationCanceledException) when (requestCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, $"Checkpoint menu failed: {checkpointId}");
        }
        finally
        {
            requestCancellation.Cancel();

            if (ReferenceEquals(activeRequestCancellation, requestCancellation))
                activeRequestCancellation = null;

            if (ReferenceEquals(pendingResult, resultSource))
            {
                pendingResult = null;
                pendingCheckpointId = null;
            }

            if (gameStateMachine.IsTopOverlay<CheckpointSavePhase>())
                await gameStateMachine.PopOverlay();

            view.Hide();
            requestCancellation.Dispose();
        }

        return result;
    }

    private void OnSlotSaveRequested(string slotId)
    {
        if (pendingResult == null || isSaveInProgress)
            return;

        CancellationTokenSource requestCancellation = activeRequestCancellation;

        if (requestCancellation == null)
            return;

        SaveToSlotAsync(
            slotId,
            pendingCheckpointId,
            pendingResult,
            requestCancellation.Token).Forget();
    }

    private async UniTask SaveToSlotAsync(
        string slotId,
        string checkpointId,
        UniTaskCompletionSource<bool> resultSource,
        CancellationToken cancellationToken)
    {
        isSaveInProgress = true;
        view.SetInteractionEnabled(false);
        bool shouldCompleteRequest = false;
        bool saveSucceeded = false;

        try
        {
            await checkpointSaveService.SaveCheckpointToSlotAsync(
                slotId,
                checkpointId,
                cancellationToken);
            shouldCompleteRequest = true;
            saveSucceeded = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            shouldCompleteRequest = true;
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

        if (shouldCompleteRequest)
            resultSource.TrySetResult(saveSucceeded);
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
        {
            activeRequestCancellation?.Cancel();
            pendingResult.TrySetResult(false);
        }
    }
}
