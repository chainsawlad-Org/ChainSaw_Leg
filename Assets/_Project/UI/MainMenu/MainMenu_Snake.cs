using System;
using UnityEngine;

public class MainMenu_Snake : MonoBehaviour
{
    public event Action StartRequested;
    public event Action ExitRequested;

    public void StartGame()
    {
        StartRequested?.Invoke();
    }

    public void EndGame()
    {
        ExitRequested?.Invoke();
    }
}
