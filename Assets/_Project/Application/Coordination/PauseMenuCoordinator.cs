using System;
using System.Collections.Generic;
using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using Cysharp.Threading.Tasks;
using Zenject;

public class PauseMenuCoordinator : IInitializable, ITickable, IDisposable
{
    private readonly PauseMenuView view;
    private readonly GameStateMachine gameStateMachine;
    private readonly PauseMenuExitCommandService exitCommandService;
    private readonly InputService inputService;
    private readonly SaveBrowserView saveBrowserView;
    private readonly ExplorationSaveCatalogService saveCatalogService;
    private readonly ExplorationGameSaveLoadService saveLoadService;
    private readonly Dictionary<string, GameSaveCatalogEntry> entriesBySlotId =
        new(StringComparer.Ordinal);
    private readonly IRuntimeErrorLogger errorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    private CancellationTokenSource catalogCancellation;
    private bool wasSaveBrowserVisible;
    private bool isLoadInProgress;

    public PauseMenuCoordinator(
        PauseMenuView view,
        GameStateMachine gameStateMachine,
        PauseMenuExitCommandService exitCommandService,
        InputService inputService,
        SaveBrowserView saveBrowserView,
        ExplorationSaveCatalogService saveCatalogService,
        ExplorationGameSaveLoadService saveLoadService,
        IRuntimeErrorLogger errorLogger)
    {
        this.view = view;
        this.gameStateMachine = gameStateMachine;
        this.exitCommandService = exitCommandService;
        this.inputService = inputService;
        this.saveBrowserView = saveBrowserView;
        this.saveCatalogService = saveCatalogService;
        this.saveLoadService = saveLoadService;
        this.errorLogger = errorLogger;
    }

    public void Initialize()
    {
        gameStateMachine.StateChanged += Refresh;
        view.ContinueClicked += OnContinueClicked;
        view.SavesClicked += OnSavesClicked;
        view.BackClicked += OnBackClicked;
        view.ExitToMainMenuClicked += OnExitToMainMenuClicked;
        saveBrowserView.LoadRequested += OnLoadRequested;

        Refresh();
    }

    public void Dispose()
    {
        gameStateMachine.StateChanged -= Refresh;
        view.ContinueClicked -= OnContinueClicked;
        view.SavesClicked -= OnSavesClicked;
        view.BackClicked -= OnBackClicked;
        view.ExitToMainMenuClicked -= OnExitToMainMenuClicked;
        saveBrowserView.LoadRequested -= OnLoadRequested;
        CancelCatalogRefresh();
        lifetimeCancellation.Cancel();
        lifetimeCancellation.Dispose();
    }

    public void Tick()
    {
        if (!gameStateMachine.IsTopOverlay<PauseMenuPhase>() &&
            !gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
            return;

        if (isLoadInProgress)
            return;

        if (inputService.PreviousPressed)
        {
            inputService.ConsumePrevious();
            view.SelectPreviousButton();
        }

        if (inputService.NextPressed)
        {
            inputService.ConsumeNext();
            view.SelectNextButton();
        }

        if (!inputService.UiSubmitPressed)
            return;

        inputService.ConsumeSubmit();
        view.ClickSelectedButton();
    }

    private void Refresh()
    {
        if (gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
        {
            view.ShowSaveBrowserPanel();

            if (!wasSaveBrowserVisible)
            {
                wasSaveBrowserVisible = true;
                StartCatalogRefresh();
            }

            return;
        }

        if (wasSaveBrowserVisible)
        {
            wasSaveBrowserVisible = false;
            CancelCatalogRefresh();
        }

        if (gameStateMachine.IsTopOverlay<PauseMenuPhase>())
        {
            view.ShowPausePanel();
            return;
        }

        view.ShowRoot(false);
    }

    private void OnContinueClicked()
    {
        if (!gameStateMachine.IsTopOverlay<PauseMenuPhase>())
            return;

        gameStateMachine.PopOverlay().Forget();
    }

    private void OnSavesClicked()
    {
        if (!gameStateMachine.IsTopOverlay<PauseMenuPhase>())
            return;

        gameStateMachine.PushOverlay<SaveBrowserPhase>().Forget();
    }

    private void OnBackClicked()
    {
        if (!gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
            return;

        gameStateMachine.PopOverlay().Forget();
    }

    private void OnExitToMainMenuClicked()
    {
        exitCommandService.RequestExitToMainMenu();
    }

    private void OnLoadRequested(string slotId)
    {
        if (isLoadInProgress ||
            !gameStateMachine.IsTopOverlay<SaveBrowserPhase>() ||
            !entriesBySlotId.TryGetValue(slotId, out GameSaveCatalogEntry entry))
            return;

        isLoadInProgress = true;
        CancelCatalogRefresh();
        view.SetSaveBrowserInteractionEnabled(false);
        LoadSaveAsync(entry, lifetimeCancellation.Token).Forget();
    }

    private void StartCatalogRefresh()
    {
        CancelCatalogRefresh();
        catalogCancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetimeCancellation.Token);
        view.SetSaveBrowserInteractionEnabled(true);
        saveBrowserView.ShowLoading();
        RefreshCatalogAsync(catalogCancellation.Token).Forget();
    }

    private async UniTask RefreshCatalogAsync(CancellationToken cancellationToken)
    {
        try
        {
            var entries = await saveCatalogService.GetCheckpointEntriesAsync(cancellationToken);

            if (!gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
                return;

            view.SetSaveBrowserInteractionEnabled(true);
            StoreEntries(entries);
            saveBrowserView.ShowEntries(SaveSlotViewDataMapper.Map(entries));
            view.SelectFirstSaveButton();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, nameof(ExplorationSaveCatalogService));

            if (gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
                saveBrowserView.ShowError("Не удалось получить список сохранений");
        }
    }

    private async UniTask LoadSaveAsync(
        GameSaveCatalogEntry entry,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new GameSaveRequest(entry.Kind, entry.SlotId, entry.CheckpointId);
            await saveLoadService.LoadAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, $"Load save failed: {entry.SlotId}");
            await EnsureSaveBrowserOpenAsync(cancellationToken);
            CancelCatalogRefresh();
            saveBrowserView.ShowError("Не удалось загрузить сохранение");
        }
        finally
        {
            isLoadInProgress = false;
            view.SetSaveBrowserInteractionEnabled(true);
        }
    }

    private async UniTask EnsureSaveBrowserOpenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (gameStateMachine.CurrentMainPhase is MainMenuPhase)
            return;

        if (!gameStateMachine.HasOverlayOfType<PauseMenuPhase>())
            await gameStateMachine.PushOverlay<PauseMenuPhase>();

        cancellationToken.ThrowIfCancellationRequested();

        if (!gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
            await gameStateMachine.PushOverlay<SaveBrowserPhase>();
    }

    private void CancelCatalogRefresh()
    {
        if (catalogCancellation == null)
            return;

        catalogCancellation.Cancel();
        catalogCancellation.Dispose();
        catalogCancellation = null;
    }

    private void StoreEntries(IReadOnlyList<GameSaveCatalogEntry> entries)
    {
        entriesBySlotId.Clear();

        foreach (GameSaveCatalogEntry entry in entries)
            entriesBySlotId[entry.SlotId] = entry;
    }
}
