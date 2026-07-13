using System;
using Zenject;

public sealed class MainMenuCoordinator : IInitializable, IDisposable
{
    private readonly MainMenuUI view;
    private readonly MainMenuStartCommandService startCommandService;
    private readonly MainMenuSaveBrowserRequestBroker saveBrowserRequestBroker;
    private readonly IApplicationQuitService applicationQuitService;

    public MainMenuCoordinator(
        MainMenuUI view,
        MainMenuStartCommandService startCommandService,
        MainMenuSaveBrowserRequestBroker saveBrowserRequestBroker,
        IApplicationQuitService applicationQuitService)
    {
        this.view = view;
        this.startCommandService = startCommandService;
        this.saveBrowserRequestBroker = saveBrowserRequestBroker;
        this.applicationQuitService = applicationQuitService;
    }

    public void Initialize()
    {
        view.StartRequested += startCommandService.RequestStartGame;
        view.LoadRequested += saveBrowserRequestBroker.RequestOpen;
        view.ExitRequested += applicationQuitService.Quit;
        saveBrowserRequestBroker.BrowserClosed += view.SelectStartButton;
    }

    public void Dispose()
    {
        view.StartRequested -= startCommandService.RequestStartGame;
        view.LoadRequested -= saveBrowserRequestBroker.RequestOpen;
        view.ExitRequested -= applicationQuitService.Quit;
        saveBrowserRequestBroker.BrowserClosed -= view.SelectStartButton;
    }
}
