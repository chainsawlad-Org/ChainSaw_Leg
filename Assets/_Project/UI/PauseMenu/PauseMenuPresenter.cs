using System;
using Cysharp.Threading.Tasks;
using Zenject;

public class PauseMenuPresenter : IInitializable, ITickable, IDisposable
{
    private readonly PauseMenuView view;
    private readonly GameStateMachine gameStateMachine;
    private readonly PauseMenuExitCommandService exitCommandService;
    private readonly InputService inputService;

    public PauseMenuPresenter(
        PauseMenuView view,
        GameStateMachine gameStateMachine,
        PauseMenuExitCommandService exitCommandService,
        InputService inputService)
    {
        this.view = view;
        this.gameStateMachine = gameStateMachine;
        this.exitCommandService = exitCommandService;
        this.inputService = inputService;
    }

    public void Initialize()
    {
        gameStateMachine.StateChanged += Refresh;
        view.ContinueClicked += OnContinueClicked;
        view.SavesClicked += OnSavesClicked;
        view.BackClicked += OnBackClicked;
        view.ExitToMainMenuClicked += OnExitToMainMenuClicked;

        Refresh();
    }

    public void Dispose()
    {
        gameStateMachine.StateChanged -= Refresh;
        view.ContinueClicked -= OnContinueClicked;
        view.SavesClicked -= OnSavesClicked;
        view.BackClicked -= OnBackClicked;
        view.ExitToMainMenuClicked -= OnExitToMainMenuClicked;
    }

    public void Tick()
    {
        if (!gameStateMachine.IsTopOverlay<PauseMenuPhase>() &&
            !gameStateMachine.IsTopOverlay<SaveBrowserPhase>())
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
            return;
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
}
