using System;

public sealed class MainMenuSaveBrowserRequestBroker : IMainMenuSaveBrowserRequestHandler
{
    public event Action OpenRequested;
    public event Action BrowserClosed;

    public void RequestOpen()
    {
        OpenRequested?.Invoke();
    }

    public void NotifyBrowserClosed()
    {
        BrowserClosed?.Invoke();
    }
}
