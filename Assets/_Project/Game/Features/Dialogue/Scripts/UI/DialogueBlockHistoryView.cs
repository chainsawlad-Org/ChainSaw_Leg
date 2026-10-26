
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueBlockHistoryView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform scrollArea;
        [SerializeField] private RectTransform blockContainer;
        [SerializeField] private GameObject dialogueBlockPrefab;

        [Header("Layout")]
        [SerializeField] private Vector2 firstBlockPosition =
            new Vector2(97f, -180f);

        [SerializeField, Min(0f)] private float blockSpacing = 45f;
        [SerializeField, Min(0.01f)] private float moveDuration = 0.25f;

        [Header("Typing")]
        [SerializeField, Min(0f)] private float typingStartDelay = 0.2f;
        [SerializeField, Min(0.01f)] private float secondsPerCharacter = 0.025f;

        [Header("Exit")]
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.2f;

        private sealed class BlockEntry
        {
            public RectTransform Rect;
            public CanvasGroup CanvasGroup;
            public TextMeshProUGUI Text;
            public float Height;
            public Tween TypingTween;
            public Tween FadeTween;
            public bool IsTyping;
        }

        private readonly List<BlockEntry> blocks = new();

        private Sequence activeMoveSequence;
        private bool isAnimatingBlocks;

        public bool IsTyping
        {
            get
            {
                foreach (BlockEntry block in blocks)
                {
                    if (block.IsTyping)
                        return true;
                }

                return false;
            }
        }

        public bool IsAnimatingBlocks => isAnimatingBlocks;

        public void ShowLine(string text)
        {
            if (dialogueBlockPrefab == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] DialogueBlockPrefab is not assigned.",
                    this);
                return;
            }

            BlockEntry entry = CreateBlock(dialogueBlockPrefab, text);

            if (entry == null)
                return;

            StartTyping(entry);
            AnimateBlocksUp();
        }

        public RectTransform ShowChoiceBlock(
            GameObject choiceBlockPrefab,
            string question)
        {
            if (choiceBlockPrefab == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] ChoiceBlockPrefab is not assigned.",
                    this);
                return null;
            }

            GameObject instance = Instantiate(
                choiceBlockPrefab,
                blockContainer,
                false);

            DialogueChoiceBlockView choiceView =
                instance.GetComponent<DialogueChoiceBlockView>();

            if (choiceView == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] " +
                    "DialogueChoiceBlock needs DialogueChoiceBlockView.",
                    instance);

                Destroy(instance);
                return null;
            }

            BlockEntry entry = RegisterBlock(
                instance,
                choiceView.QuestionText,
                question);

            if (entry == null)
                return null;

            StartTyping(entry);
            AnimateBlocksUp();

            return choiceView.ChoicesContainer;
        }

        private BlockEntry CreateBlock(GameObject prefab, string text)
        {
            if (scrollArea == null || blockContainer == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] ScrollArea or BlockContainer is not assigned.",
                    this);
                return null;
            }

            GameObject instance = Instantiate(
                prefab,
                blockContainer,
                false);

            TextMeshProUGUI textComponent =
                instance.GetComponentInChildren<TextMeshProUGUI>(true);

            if (textComponent == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] Prefab needs a TextMeshProUGUI.",
                    instance);

                Destroy(instance);
                return null;
            }

            return RegisterBlock(instance, textComponent, text);
        }

        private BlockEntry RegisterBlock(
            GameObject instance,
            TextMeshProUGUI textComponent,
            string text)
        {
            if (scrollArea == null || blockContainer == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] References are not assigned.",
                    this);

                Destroy(instance);
                return null;
            }

            RectTransform rect = instance.GetComponent<RectTransform>();

            if (rect == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] Prefab root needs a RectTransform.",
                    instance);

                Destroy(instance);
                return null;
            }

            if (textComponent == null)
            {
                Debug.LogError(
                    "[DialogueBlockHistoryView] Question text is not assigned.",
                    instance);

                Destroy(instance);
                return null;
            }

            RectTransform prefabRect =
                instance.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = prefabRect.sizeDelta;

            CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = instance.AddComponent<CanvasGroup>();

            textComponent.text = text ?? string.Empty;
            textComponent.maxVisibleCharacters = 0;
            textComponent.ForceMeshUpdate();

            BlockEntry entry = new BlockEntry
            {
                Rect = rect,
                CanvasGroup = canvasGroup,
                Text = textComponent,
                Height = rect.rect.height
            };

            blocks.Insert(0, entry);

            rect.anchoredPosition = firstBlockPosition;
            canvasGroup.alpha = 1f;

            Debug.Log(
                $"[DialogueBlockHistoryView] Block size: " +
                $"{rect.rect.width} x {rect.rect.height}; " +
                $"Container size: {blockContainer.rect.width} x " +
                $"{blockContainer.rect.height}",
                instance);

            return entry;
        }

        public void CompleteTyping()
        {
            foreach (BlockEntry block in blocks)
            {
                if (!block.IsTyping)
                    continue;

                block.TypingTween?.Complete();
            }
        }

        public void ClearImmediate()
        {
            activeMoveSequence?.Kill();
            activeMoveSequence = null;
            isAnimatingBlocks = false;

            foreach (BlockEntry block in blocks)
            {
                block.TypingTween?.Kill();
                block.FadeTween?.Kill();
                block.IsTyping = false;
            }

            blocks.Clear();

            if (blockContainer == null)
                return;

            for (int i = blockContainer.childCount - 1; i >= 0; i--)
                Destroy(blockContainer.GetChild(i).gameObject);
        }

        private void StartTyping(BlockEntry block)
        {
            int characterCount = block.Text.textInfo.characterCount;

            if (characterCount == 0)
            {
                block.Text.maxVisibleCharacters = int.MaxValue;
                return;
            }

            block.IsTyping = true;

            float duration = characterCount * secondsPerCharacter;

            Sequence typingSequence = DOTween.Sequence();

            block.TypingTween = typingSequence
                .AppendInterval(typingStartDelay)
                .Append(
                    DOTween.To(
                        () => 0,
                        visibleCharacters =>
                            block.Text.maxVisibleCharacters = visibleCharacters,
                        characterCount,
                        duration
                    ).SetEase(Ease.Linear)
                )
                .SetTarget(block.Text)
                .OnComplete(() =>
                {
                    block.Text.maxVisibleCharacters = int.MaxValue;
                    block.IsTyping = false;
                    block.TypingTween = null;
                });
        }

        private void AnimateBlocksUp()
        {
            activeMoveSequence?.Kill();

            isAnimatingBlocks = true;

            Sequence sequence = DOTween.Sequence();
            activeMoveSequence = sequence;

            float targetY = firstBlockPosition.y;

            for (int i = 0; i < blocks.Count; i++)
            {
                BlockEntry block = blocks[i];

                if (block.Rect == null)
                    continue;

                sequence.Join(
                    block.Rect
                        .DOAnchorPosY(targetY, moveDuration)
                        .SetEase(Ease.OutCubic));

                targetY += block.Height + blockSpacing;
            }

            sequence.OnComplete(() =>
            {
                isAnimatingBlocks = false;
                activeMoveSequence = null;
                RemoveBlocksAboveView();
            });
        }

        private void RemoveBlocksAboveView()
        {
            float viewHeight = scrollArea.rect.height;

            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                BlockEntry block = blocks[i];

                if (block.Rect == null)
                {
                    blocks.RemoveAt(i);
                    continue;
                }

                if (block.Rect.anchoredPosition.y <=
                    viewHeight + block.Height)
                {
                    continue;
                }

                blocks.RemoveAt(i);

                block.TypingTween?.Kill();

                block.FadeTween = block.CanvasGroup
                    .DOFade(0f, fadeDuration)
                    .OnComplete(() =>
                    {
                        if (block.Rect != null)
                            Destroy(block.Rect.gameObject);
                    });
            }
        }

        private void OnDestroy()
        {
            activeMoveSequence?.Kill();

            foreach (BlockEntry block in blocks)
            {
                block.TypingTween?.Kill();
                block.FadeTween?.Kill();
            }
        }
    }
}
