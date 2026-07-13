using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button exitButton;

    public event Action StartRequested;
    public event Action LoadRequested;
    public event Action ExitRequested;

    private void Start()
    {
        ConfigureNavigation();
        SelectStartButton();
    }

    public void StartGame()
    {
        StartRequested?.Invoke();
    }

    public void LoadGame()
    {
        LoadRequested?.Invoke();
    }

    public void ExitGame()
    {
        ExitRequested?.Invoke();
    }

    private void ConfigureNavigation()
    {
        SetVerticalNavigation(startButton, null, loadButton);
        SetVerticalNavigation(loadButton, startButton, exitButton);
        SetVerticalNavigation(exitButton, loadButton, null);
    }

    public void SelectStartButton()
    {
        if (EventSystem.current == null || startButton == null)
            return;

        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        startButton.Select();
    }

    private static void SetVerticalNavigation(Button button, Button up, Button down)
    {
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = up;
        navigation.selectOnDown = down;
        navigation.selectOnLeft = null;
        navigation.selectOnRight = null;
        button.navigation = navigation;
    }

}
