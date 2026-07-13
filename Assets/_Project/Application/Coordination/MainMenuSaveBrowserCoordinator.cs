using System;
using System.Collections.Generic;
using System.Threading;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using Cysharp.Threading.Tasks;
using Zenject;

public sealed class MainMenuSaveBrowserCoordinator :
    IInitializable,
    ITickable,
    IDisposable
{
    private readonly PauseMenuView view;
    private readonly SaveBrowserView saveBrowserView;
    private readonly ExplorationSaveCatalogService saveCatalogService;
    private readonly ExplorationGameSaveLoadService saveLoadService;
    private readonly GameStateMachine gameStateMachine;
    private readonly InputService inputService;
    private readonly MainMenuSaveBrowserRequestBroker requestBroker;
    private readonly Dictionary<string, GameSaveCatalogEntry> entriesBySlotId =
        new(StringComparer.Ordinal);
    private readonly IRuntimeErrorLogger errorLogger;
    private readonly CancellationTokenSource lifetimeCancellation = new();

    private CancellationTokenSource catalogCancellation;
    private bool isOpen;
    private bool isLoadInProgress;

    public MainMenuSaveBrowserCoordinator(
        PauseMenuView view,
        SaveBrowserView saveBrowserView,
        ExplorationSaveCatalogService saveCatalogService,
        ExplorationGameSaveLoadService saveLoadService,
        GameStateMachine gameStateMachine,
        InputService inputService,
        MainMenuSaveBrowserRequestBroker requestBroker,
        IRuntimeErrorLogger errorLogger)
    {
        this.view = view;
        this.saveBrowserView = saveBrowserView;
        this.saveCatalogService = saveCatalogService;
        this.saveLoadService = saveLoadService;
        this.gameStateMachine = gameStateMachine;
        this.inputService = inputService;
        this.requestBroker = requestBroker;
        this.errorLogger = errorLogger;
    }

    public void Initialize()
    {
        requestBroker.OpenRequested += RequestOpen;
        view.BackClicked += OnBackClicked;
        saveBrowserView.LoadRequested += OnLoadRequested;
    }

    public void Dispose()
    {
        requestBroker.OpenRequested -= RequestOpen;
        view.BackClicked -= OnBackClicked;
        saveBrowserView.LoadRequested -= OnLoadRequested;
        CancelCatalogRefresh();
        lifetimeCancellation.Cancel();
        lifetimeCancellation.Dispose();
    }

    public void RequestOpen()
    {
        if (isOpen || gameStateMachine.CurrentMainPhase is not MainMenuPhase)
            return;

        isOpen = true;
        view.ShowSaveBrowserPanel();
        StartCatalogRefresh();
    }

    public void Tick()
    {
        if (!isOpen || isLoadInProgress)
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

    private void OnBackClicked()
    {
        if (!isOpen || isLoadInProgress)
            return;

        Close();
        requestBroker.NotifyBrowserClosed();
    }

    private void OnLoadRequested(string slotId)
    {
        if (!isOpen ||
            isLoadInProgress ||
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
            var entries = await saveCatalogService.GetEntriesAsync(cancellationToken);

            if (!isOpen)
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
            errorLogger.LogException(exception, nameof(MainMenuSaveBrowserCoordinator));

            if (isOpen)
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
            Close();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            errorLogger.LogException(exception, $"Main menu load failed: {entry.SlotId}");

            if (isOpen)
                saveBrowserView.ShowError("Не удалось загрузить сохранение");
        }
        finally
        {
            isLoadInProgress = false;

            if (isOpen)
                view.SetSaveBrowserInteractionEnabled(true);
        }
    }

    private void Close()
    {
        CancelCatalogRefresh();
        isOpen = false;
        view.ShowRoot(false);
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
