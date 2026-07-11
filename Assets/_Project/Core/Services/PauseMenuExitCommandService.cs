using System;

public class PauseMenuExitCommandService
{
    public event Action ExitToMainMenuRequested;

    public void RequestExitToMainMenu()
    {
        ExitToMainMenuRequested?.Invoke();
    }
}
