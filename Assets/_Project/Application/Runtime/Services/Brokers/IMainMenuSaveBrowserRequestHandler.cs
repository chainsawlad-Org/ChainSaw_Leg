using System;

public interface IMainMenuSaveBrowserRequestHandler
{
    event Action BrowserClosed;

    void RequestOpen();
}
