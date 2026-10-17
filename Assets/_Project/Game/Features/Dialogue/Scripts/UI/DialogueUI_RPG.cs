using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Dialogue
{
    public class DialogueUI_RPG : MonoBehaviour
    {
        private static readonly Color ChoiceNormalColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ChoiceHighlightedColor = new Color(0.92f, 0.95f, 1f, 1f);
        private static readonly Color ChoicePressedColor = new Color(0.8f, 0.86f, 0.95f, 1f);
        private static readonly Color ChoiceSelectedColor = new Color(0.67f, 0.82f, 1f, 1f);
        private static readonly Color ChoiceOutlineColor = new Color(0.16f, 0.44f, 0.82f, 1f);

        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Continue Hint")]
        [SerializeField] private GameObject continueHint;

        [Header("Choices")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;

        private readonly List<Button> activeChoiceButtons = new();
        private readonly List<Outline> activeChoiceOutlines = new();
        private List<DialogueChoice> activeChoices = new();
        
        private int selectedChoiceIndex = -1;
        // private DialogueManager dialogueManager;

        public bool IsVisible => root != null && root.activeSelf;

        public bool HasChoices => activeChoiceButtons.Count > 0;

        public int SelectedChoiceIndex => selectedChoiceIndex;

        private void Awake()
        {
            // root.SetActive(false);
            Hide();
        }

        // public void SetDialogueManager(DialogueManager manager)
        // {
        //     dialogueManager = manager;
        // }
        
        public void ShowRoot()
        {
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void ShowText(string text)
        {
            ShowRoot();

            if (dialogueText != null)
                dialogueText.text = text ?? string.Empty;
            
            ClearChoices();
            
            if(continueHint != null)
                continueHint.SetActive(true);
        }
        
        public void ShowNode(DialogueNode node)
        {
            if (node == null)
            {
                Hide();
                return;
            }

            if (node.Type == DialogueNodeType.Line)
            {
                ShowText(node.Text);
                return;
            }

            if (node.Type == DialogueNodeType.Choice)
            {
                ShowChoices(node.Choices);
                return;
            }
        }

        public void ShowChoices(IReadOnlyList<DialogueChoice> choices)
        {
            ShowRoot();
            
            ClearChoices();
            
            if (dialogueText != null)
                dialogueText.text = string.Empty;

            if (continueHint != null)
                continueHint.SetActive(false);

            if (choices == null || choices.Count == 0)
                return;

            foreach (DialogueChoice choice in choices)
            {
                if (choice == null)
                    continue;

                activeChoices.Add(choice);
            }

            for (int i = 0; i < activeChoices.Count; i++)
            {
                int index = i;

                Button button = Instantiate(
                    choiceButtonPrefab,
                    choicesContainer);

                ConfigureChoiceButtonVisuals(button);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>();

                if (label != null)
                    label.text = activeChoices[index].Text;

                button.onClick.AddListener(() =>
                {
                    SelectChoice(index);
                });

                activeChoiceButtons.Add(button);

                Outline outline = button.GetComponent<Outline>();
                activeChoiceOutlines.Add(outline);
            }

            if (activeChoiceButtons.Count > 0)
                SelectChoice(0);
        }
        
        // public void ShowText(string t)
        // {
        //     root.SetActive(true);
        //     dialogueText.text = t;
        //     ClearChoices();
        //     continueHint.SetActive(true);
        // }

        // public void ShowChoices(List<DialogueChoice> choices)
        // {
        //     ClearChoices();
        //     activeChoices = choices;
        //
        //     continueHint.SetActive(false);
        //
        //     for (int i = 0; i < choices.Count; i++)
        //     {
        //         int index = i;
        //         var btn = Instantiate(choiceButtonPrefab, choicesContainer);
        //         ConfigureChoiceButtonVisuals(btn);
        //
        //         btn.GetComponentInChildren<TextMeshProUGUI>().text = choices[i].text;
        //         btn.onClick.AddListener(() =>
        //         {
        //             SelectChoice(index);
        //             dialogueManager?.Choose(index, activeChoices);
        //         });
        //
        //         activeChoiceButtons.Add(btn);
        //         activeChoiceOutlines.Add(btn.GetComponent<Outline>());
        //     }
        //
        //     SelectChoice(0);
        // }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);

            ClearChoices();

            if (dialogueText != null)
                dialogueText.text = string.Empty;

            if (continueHint != null)
                continueHint.SetActive(false);
        }

        private void ClearChoices()
        {
            activeChoices.Clear();
            
            selectedChoiceIndex = -1;
            
            activeChoiceButtons.Clear();
            activeChoiceOutlines.Clear();
            
            EventSystem.current?.SetSelectedGameObject(null);
            
            if (choicesContainer == null)
                return;

            foreach (Transform child in choicesContainer)
                Destroy(child.gameObject);
        }

        public void SelectPreviousChoice()
        {
            if (activeChoiceButtons.Count == 0)
                return;

            int nextIndex = selectedChoiceIndex <= 0 ? 0 : selectedChoiceIndex - 1;
            SelectChoice(nextIndex);
        }

        public void SelectNextChoice()
        {
            if (activeChoiceButtons.Count == 0)
                return;

            int nextIndex = selectedChoiceIndex < 0
                ? 0
                : Mathf.Min(activeChoiceButtons.Count - 1, selectedChoiceIndex + 1);

            SelectChoice(nextIndex);
        }

        // public void SubmitCurrentChoice()
        // {
        //     if (activeChoiceButtons.Count == 0 || activeChoices == null)
        //         return;
        //
        //     if (selectedChoiceIndex < 0)
        //         SelectChoice(0);
        //
        //     dialogueManager?.Choose(selectedChoiceIndex, activeChoices);
        // }

        private void SelectChoice(int index)
        {
            if (index < 0 || index >= activeChoiceButtons.Count)
                return;

            selectedChoiceIndex = index;
            RefreshChoiceSelectionVisuals();
        }

        private static void ConfigureChoiceButtonVisuals(Button button)
        {
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            ColorBlock colors = button.colors;
            
            colors.normalColor = ChoiceNormalColor;
            colors.highlightedColor = ChoiceHighlightedColor;
            colors.pressedColor = ChoicePressedColor;
            colors.selectedColor = ChoiceSelectedColor;
            
            button.colors = colors;

            Outline outline = button.GetComponent<Outline>();

            if (outline == null)
                outline = button.gameObject.AddComponent<Outline>();

            outline.effectColor = ChoiceOutlineColor;
            outline.effectDistance = new Vector2(3f, -3f);
            outline.useGraphicAlpha = false;
            outline.enabled = false;
        }

        // private void RefreshChoiceSelectionVisuals()
        // {
        //     for (int i = 0; i < activeChoiceButtons.Count; i++)
        //     {
        //         if (i >= activeChoiceOutlines.Count || activeChoiceOutlines[i] == null)
        //             continue;
        //
        //         Graphic targetGraphic = activeChoiceButtons[i].targetGraphic;
        //
        //         if (targetGraphic != null)
        //             targetGraphic.color = i == selectedChoiceIndex ? ChoiceSelectedColor : ChoiceNormalColor;
        //
        //         activeChoiceOutlines[i].enabled = i == selectedChoiceIndex;
        //     }

        private void RefreshChoiceSelectionVisuals()
        {
            for (int i = 0; i < activeChoiceButtons.Count; i++)
            {
                Button button = activeChoiceButtons[i];

                if (button == null)
                    continue;

                bool isSelected = i == selectedChoiceIndex;

                if (button.targetGraphic != null)
                {
                    button.targetGraphic.color = isSelected
                        ? ChoiceSelectedColor
                        : ChoiceNormalColor;
                }

                if (i < activeChoiceOutlines.Count &&
                    activeChoiceOutlines[i] != null)
                {
                    activeChoiceOutlines[i].enabled = isSelected;
                }
            }
        }

        public bool TryGetSelectedChoiceId(out string choiceId)
        {
            choiceId = null;

            if (selectedChoiceIndex < 0 ||
                selectedChoiceIndex >= activeChoices.Count)
            {
                return false;
            }

            DialogueChoice choice = activeChoices[selectedChoiceIndex];

            if (choice == null || string.IsNullOrWhiteSpace(choice.Id))
                return false;

            choiceId = choice.Id;
            return true;
        }
    }
}
