using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject saveBrowserPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button savesButton;
    [SerializeField] private Button exitToMainMenuButton;
    [SerializeField] private Button backButton;

    public event Action ContinueClicked;
    public event Action SavesClicked;
    public event Action ExitToMainMenuClicked;
    public event Action BackClicked;

    private bool navigationStateCaptured;
    private bool previousSendNavigationEvents;

    public void SetReferences(
        GameObject rootPanel,
        GameObject pausePanel,
        GameObject saveBrowserPanel,
        Button continueButton,
        Button savesButton,
        Button exitToMainMenuButton,
        Button backButton)
    {
        this.rootPanel = rootPanel;
        this.pausePanel = pausePanel;
        this.saveBrowserPanel = saveBrowserPanel;
        this.continueButton = continueButton;
        this.savesButton = savesButton;
        this.exitToMainMenuButton = exitToMainMenuButton;
        this.backButton = backButton;

        ConfigureNavigation();
    }

    private void OnEnable()
    {
        continueButton.onClick.AddListener(HandleContinueClicked);
        savesButton.onClick.AddListener(HandleSavesClicked);
        exitToMainMenuButton.onClick.AddListener(HandleExitToMainMenuClicked);
        backButton.onClick.AddListener(HandleBackClicked);
    }

    private void OnDisable()
    {
        continueButton.onClick.RemoveListener(HandleContinueClicked);
        savesButton.onClick.RemoveListener(HandleSavesClicked);
        exitToMainMenuButton.onClick.RemoveListener(HandleExitToMainMenuClicked);
        backButton.onClick.RemoveListener(HandleBackClicked);
    }

    public void ShowRoot(bool isVisible)
    {
        rootPanel.SetActive(isVisible);

        if (isVisible)
        {
            SetBuiltInNavigationEnabled(false);
            return;
        }

        RestoreBuiltInNavigation();
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void ShowPausePanel()
    {
        rootPanel.SetActive(true);
        SetBuiltInNavigationEnabled(false);
        pausePanel.SetActive(true);
        saveBrowserPanel.SetActive(false);
        SelectButton(continueButton);
    }

    public void ShowSaveBrowserPanel()
    {
        rootPanel.SetActive(true);
        SetBuiltInNavigationEnabled(false);
        pausePanel.SetActive(false);
        saveBrowserPanel.SetActive(true);
        SelectButton(backButton);
    }

    public void SelectPreviousButton()
    {
        MoveSelection(-1);
    }

    public void SelectNextButton()
    {
        MoveSelection(1);
    }

    public void ClickSelectedButton()
    {
        if (EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        Button selectedButton = selectedObject != null ? selectedObject.GetComponent<Button>() : null;

        if (selectedButton != null && selectedButton.IsInteractable())
            selectedButton.onClick.Invoke();
    }

    private void HandleContinueClicked()
    {
        ContinueClicked?.Invoke();
    }

    private void HandleSavesClicked()
    {
        SavesClicked?.Invoke();
    }

    private void HandleExitToMainMenuClicked()
    {
        ExitToMainMenuClicked?.Invoke();
    }

    private void HandleBackClicked()
    {
        BackClicked?.Invoke();
    }

    private void ConfigureNavigation()
    {
        SetVerticalNavigation(continueButton, null, savesButton);
        SetVerticalNavigation(savesButton, continueButton, exitToMainMenuButton);
        SetVerticalNavigation(exitToMainMenuButton, savesButton, null);
        SetVerticalNavigation(backButton, null, null);
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

    private static void SelectButton(Button button)
    {
        if (button == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
        button.Select();
    }

    private void MoveSelection(int direction)
    {
        if (saveBrowserPanel.activeSelf)
        {
            SelectButton(backButton);
            return;
        }

        Button[] buttons = { continueButton, savesButton, exitToMainMenuButton };
        GameObject selectedObject = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;
        int currentIndex = Array.FindIndex(buttons, button => button != null && button.gameObject == selectedObject);
        int nextIndex = currentIndex < 0
            ? 0
            : Mathf.Clamp(currentIndex + direction, 0, buttons.Length - 1);

        SelectButton(buttons[nextIndex]);
    }

    private void SetBuiltInNavigationEnabled(bool isEnabled)
    {
        if (EventSystem.current == null)
            return;

        if (!navigationStateCaptured)
        {
            previousSendNavigationEvents = EventSystem.current.sendNavigationEvents;
            navigationStateCaptured = true;
        }

        EventSystem.current.sendNavigationEvents = isEnabled;
    }

    private void RestoreBuiltInNavigation()
    {
        if (!navigationStateCaptured || EventSystem.current == null)
            return;

        EventSystem.current.sendNavigationEvents = previousSendNavigationEvents;
        navigationStateCaptured = false;
    }
}
