using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Dialogue
{
    public class DialogueUI_RPG : MonoBehaviour, IDialogueView
    {
        [System.Serializable]
        private class CharacterVisualConfig
        {
            public string characterId;
            public Sprite backdropSprite;
            public Sprite defaultPortrait;
        }

        [SerializeField, Min(0f)] private float portraitEnterDelay = 0.08f;
        [SerializeField, Min(0.01f)] private float exitDuration = 0.3f;

        [SerializeField, Min(0.01f)] private float backdropEnterDuration = 0.45f;

        [SerializeField, Min(0.01f)] private float portraitEnterDuration = 0.35f;

        [SerializeField] private Ease backdropEnterEase = Ease.OutCubic;
        [SerializeField] private Ease portraitEnterEase = Ease.OutBack;
        [SerializeField] private Ease exitEase = Ease.InCubic;

        [Header("Root")] [SerializeField] private GameObject root;

        [Header("Text")] [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Dialogue Blocks")] [SerializeField]
        private RectTransform dialogueScrollArea;

        [SerializeField] private RectTransform dialogueBlockContainer;

        [SerializeField] private GameObject dialogueBlockPrefab;
        [SerializeField] private GameObject choiceBlockPrefab;

        [Header("Dialogue History")] [SerializeField]
        private DialogueBlockHistoryView dialogueBlockHistoryView;

        [Header("Continue Hint")] [SerializeField]
        private GameObject continueHint;

        [Header("Choices")] [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Character Presentation")] [SerializeField]
        private Image leftCharacterBackdrop;

        [SerializeField] private Image leftCharacterPortrait;
        [SerializeField] private Image rightCharacterBackdrop;
        [SerializeField] private Image rightCharacterPortrait;

        [Header("Character Configurations")] [SerializeField]
        private List<CharacterVisualConfig> characterVisuals = new();

        [SerializeField] private string protagonistId = "Misty";

        private RectTransform leftBackdropRect;
        private RectTransform leftPortraitRect;
        private RectTransform rightBackdropRect;
        private RectTransform rightPortraitRect;

        private Vector2 leftBackdropPosition;
        private Vector2 leftPortraitPosition;
        private Vector2 rightBackdropPosition;
        private Vector2 rightPortraitPosition;

        private float offscreenDistance;
        private Sequence activeAnimation;
        private bool isAnimating;

        private static readonly Color ChoiceNormalColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ChoiceHighlightedColor = new Color(0.92f, 0.95f, 1f, 1f);
        private static readonly Color ChoicePressedColor = new Color(0.8f, 0.86f, 0.95f, 1f);
        private static readonly Color ChoiceSelectedColor = new Color(0.67f, 0.82f, 1f, 1f);
        private static readonly Color ChoiceOutlineColor = new Color(0.16f, 0.44f, 0.82f, 1f);

        private readonly List<Button> activeChoiceButtons = new();
        private readonly List<Outline> activeChoiceOutlines = new();
        private List<DialogueChoice> activeChoices = new();

        private int selectedChoiceIndex = -1;

        public bool IsVisible => root != null && root.activeSelf;
        public bool HasChoices => activeChoiceButtons.Count > 0;
        public int SelectedChoiceIndex => selectedChoiceIndex;

        public bool IsTyping =>
            dialogueBlockHistoryView != null &&
            dialogueBlockHistoryView.IsTyping;

        public bool IsAnimatingBlocks =>
            dialogueBlockHistoryView != null &&
            dialogueBlockHistoryView.IsAnimatingBlocks;

        public void BeginDialogue(Dialogue dialogue)
        {
            dialogueBlockHistoryView?.ClearImmediate();

            ClearCharacterVisual(
                leftCharacterBackdrop,
                leftCharacterPortrait);

            ClearCharacterVisual(
                rightCharacterBackdrop,
                rightCharacterPortrait);

            string opponentId = FindConversationPartner(dialogue);

            Debug.Log(
                $"[DialogueUI_RPG] Dialogue: {dialogue?.Id}, " +
                $"Protagonist: {protagonistId}, " +
                $"Opponent: {opponentId}",
                this);

            if (!string.IsNullOrWhiteSpace(opponentId))
            {
                ApplyCharacterVisuals(opponentId, true);
            }

            ApplyCharacterVisuals(protagonistId, false);

            ShowRoot();
            PlayEnterAnimation();
        }

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

            if (continueHint != null)
                continueHint.SetActive(true);
        }

        public void ShowNode(DialogueNode node)
        {
            if (node == null)
            {
                Hide();
                return;
            }

            switch (node.Type)
            {
                case DialogueNodeType.Line:
                    ShowRoot();

                    if (dialogueBlockHistoryView != null)
                        dialogueBlockHistoryView.ShowLine(node.Text);
                    else
                        ShowText(node.Text);

                    break;

                case DialogueNodeType.Choice:
                    ShowRoot();

                    RectTransform targetChoicesContainer = null;

                    if (dialogueBlockHistoryView != null)
                    {
                        targetChoicesContainer =
                            dialogueBlockHistoryView.ShowChoiceBlock(
                                choiceBlockPrefab,
                                node.Text);
                    }

                    ShowChoices(node.Choices, targetChoicesContainer);
                    break;

                default:
                    Debug.LogError(
                        $"Unsupported dialogue node type: {node.Type}",
                        this);
                    break;
            }
        }

        public void ShowChoices(IReadOnlyList<DialogueChoice> choices)
        {
            ShowChoices(choices, choicesContainer as RectTransform);
        }

        private void ShowChoices(
            IReadOnlyList<DialogueChoice> choices,
            RectTransform targetContainer)
        {
            ShowRoot();
            ClearChoices();

            if (continueHint != null)
                continueHint.SetActive(false);

            if (choices == null || choices.Count == 0)
                return;

            if (targetContainer == null)
            {
                Debug.LogError(
                    "[DialogueUI_RPG] ChoicesContainer is not assigned " +
                    "or DialogueChoiceBlock could not be created.",
                    this);
                return;
            }

            foreach (DialogueChoice choice in choices)
            {
                if (choice != null)
                    activeChoices.Add(choice);
            }

            for (int i = 0; i < activeChoices.Count; i++)
            {
                int index = i;

                Button button = Instantiate(
                    choiceButtonPrefab,
                    targetContainer,
                    false);

                RectTransform buttonRect =
                    button.GetComponent<RectTransform>();

                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.pivot = new Vector2(0.5f, 0.5f);

                buttonRect.anchoredPosition =
                    GetChoicePosition(i, activeChoices.Count);

                ConfigureChoiceButtonVisuals(button);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>();

                if (label != null)
                    label.text = activeChoices[index].Text;

                button.onClick.AddListener(() => SelectChoice(index));

                activeChoiceButtons.Add(button);

                Outline outline = button.GetComponent<Outline>();
                activeChoiceOutlines.Add(outline);
            }

            if (activeChoiceButtons.Count > 0)
                SelectChoice(0);
        }

        private static Vector2 GetChoicePosition(int index, int count)
        {
            if (count == 1)
                return new Vector2(-12f, 69f);

            if (count == 2)
            {
                return index == 0
                    ? new Vector2(-147f, 69f)
                    : new Vector2(198f, 69f);
            }

            if (count == 3)
            {
                switch (index)
                {
                    case 0: return new Vector2(-293f, 69f);
                    case 1: return new Vector2(-12f, 69f);
                    case 2: return new Vector2(262f, 69f);
                }
            }

            float spacing = 300f;
            float startX = -((count - 1) * spacing) / 2f;

            return new Vector2(startX + index * spacing, 69f);
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

        public void CompleteTyping()
        {
            dialogueBlockHistoryView?.CompleteTyping();
        }

        public void Hide()
        {
            dialogueBlockHistoryView?.ClearImmediate();

            if (root != null)
                root.SetActive(false);

            ClearChoices();

            if (dialogueText != null)
                dialogueText.text = string.Empty;

            if (continueHint != null)
                continueHint.SetActive(false);
        }

        private void Awake()
        {
            leftBackdropRect = leftCharacterBackdrop.rectTransform;
            leftPortraitRect = leftCharacterPortrait.rectTransform;
            rightBackdropRect = rightCharacterBackdrop.rectTransform;
            rightPortraitRect = rightCharacterPortrait.rectTransform;

            leftBackdropPosition = leftBackdropRect.anchoredPosition;
            leftPortraitPosition = leftPortraitRect.anchoredPosition;
            rightBackdropPosition = rightBackdropRect.anchoredPosition;
            rightPortraitPosition = rightPortraitRect.anchoredPosition;

            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null
                ? canvas.GetComponent<RectTransform>()
                : null;

            offscreenDistance = canvasRect != null
                ? canvasRect.rect.width + 100f
                : Screen.width + 100f;

            Hide();
        }

        private void ApplyCharacterVisuals(string characterId, bool isLeftCharacter)
        {
            CharacterVisualConfig config = characterVisuals.Find(character => character.characterId == characterId);

            if (config == null)
            {
                Debug.LogWarning(
                    $"[DialogueUI_RPG] Character configuration not found: {characterId}",
                    this);

                return;
            }

            Image backdrop = isLeftCharacter ? leftCharacterBackdrop : rightCharacterBackdrop;
            Image portrait = isLeftCharacter ? leftCharacterPortrait : rightCharacterPortrait;

            if (backdrop != null)
            {
                backdrop.sprite = config.backdropSprite;
                backdrop.enabled = config.backdropSprite != null;
            }

            if (portrait != null)
            {
                portrait.sprite = config.defaultPortrait;
                portrait.enabled = config.defaultPortrait != null;
            }
        }

        private void ClearChoices()
        {
            foreach (Button button in activeChoiceButtons)
            {
                if (button == null)
                    continue;

                button.interactable = false;
                button.onClick.RemoveAllListeners();
            }

            activeChoices.Clear();
            activeChoiceButtons.Clear();
            activeChoiceOutlines.Clear();

            selectedChoiceIndex = -1;

            EventSystem.current?.SetSelectedGameObject(null);
        }

        private static void ClearCharacterVisual(
            Image backdrop,
            Image portrait)
        {
            if (backdrop != null)
                backdrop.enabled = false;

            if (portrait != null)
            {
                portrait.sprite = null;
                portrait.enabled = false;
            }
        }

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

        private string FindConversationPartner(Dialogue dialogue)
        {
            if (dialogue == null)
                return null;

            foreach (DialogueNode node in dialogue.Nodes.Values)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.Speaker))
                    continue;

                if (node.Speaker == protagonistId)
                    continue;

                return node.Speaker;
            }

            return null;
        }

        private void SetInitialAnimationPositions()
        {
            leftBackdropRect.anchoredPosition =
                leftBackdropPosition + Vector2.left * offscreenDistance;

            leftPortraitRect.anchoredPosition =
                leftPortraitPosition + Vector2.left * offscreenDistance;

            rightBackdropRect.anchoredPosition =
                rightBackdropPosition + Vector2.right * offscreenDistance;

            rightPortraitRect.anchoredPosition =
                rightPortraitPosition + Vector2.right * offscreenDistance;
        }

        private void RestoreAnimationPositions()
        {
            leftBackdropRect.anchoredPosition = leftBackdropPosition;
            leftPortraitRect.anchoredPosition = leftPortraitPosition;
            rightBackdropRect.anchoredPosition = rightBackdropPosition;
            rightPortraitRect.anchoredPosition = rightPortraitPosition;
        }

        private void PlayEnterAnimation()
        {
            activeAnimation?.Kill();
            isAnimating = true;

            SetInitialAnimationPositions();

            activeAnimation = DOTween.Sequence();

            activeAnimation.Append(
                leftBackdropRect
                    .DOAnchorPos(leftBackdropPosition, backdropEnterDuration)
                    .SetEase(backdropEnterEase));

            activeAnimation.Join(
                rightBackdropRect
                    .DOAnchorPos(rightBackdropPosition, backdropEnterDuration)
                    .SetEase(backdropEnterEase));

            activeAnimation.Insert(
                portraitEnterDelay,
                leftPortraitRect
                    .DOAnchorPos(leftPortraitPosition, portraitEnterDuration)
                    .SetEase(portraitEnterEase));

            activeAnimation.Join(
                rightPortraitRect
                    .DOAnchorPos(rightPortraitPosition, portraitEnterDuration)
                    .SetEase(portraitEnterEase));

            activeAnimation.OnComplete(() =>
            {
                isAnimating = false;
                RestoreAnimationPositions();
            });
        }

        public async UniTask HideAnimatedAsync()
        {
            if (root == null || !root.activeSelf)
                return;

            activeAnimation?.Kill();
            isAnimating = true;

            activeAnimation = DOTween.Sequence();

            activeAnimation.Join(
                leftBackdropRect
                    .DOAnchorPos(
                        leftBackdropPosition + Vector2.left * offscreenDistance,
                        exitDuration)
                    .SetEase(exitEase));

            activeAnimation.Join(
                leftPortraitRect
                    .DOAnchorPos(
                        leftPortraitPosition + Vector2.left * offscreenDistance,
                        exitDuration)
                    .SetEase(exitEase));

            activeAnimation.Join(
                rightBackdropRect
                    .DOAnchorPos(
                        rightBackdropPosition + Vector2.right * offscreenDistance,
                        exitDuration)
                    .SetEase(exitEase));

            activeAnimation.Join(
                rightPortraitRect
                    .DOAnchorPos(
                        rightPortraitPosition + Vector2.right * offscreenDistance,
                        exitDuration)
                    .SetEase(exitEase));

            await activeAnimation.AsyncWaitForCompletion();

            isAnimating = false;
            Hide();
            RestoreAnimationPositions();
        }
    }
}