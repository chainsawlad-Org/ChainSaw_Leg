
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class CameraFixInputController : MonoBehaviour
    {
        [Header("Phases")]
        [SerializeField] private List<CameraFixPhaseSetup> phases = new();

        [Header("Transition")]
        [SerializeField] private CanvasGroup transitionCanvasGroup;
        [SerializeField] private float fadeDuration = 0.15f;

        [Header("Selection Frame")]
        [SerializeField] private GameObject selectionFramePrefab;

        private readonly List<CircuitBlock> navigationOrder = new();
        private readonly List<CircuitBlock> availableBlockPrefabs = new();
        private readonly List<CircuitBlock> inventoryDisplayBlocks = new();

        private readonly Dictionary<CircuitBlock, Sprite> originalSprites = new();

        private GameObject selectionFrameInstance;
        private SpriteRenderer selectionFrameRenderer;

        private int currentPhaseIndex;
        private int selectedIndex = -1;
        private int selectedInventoryIndex;

        private bool isChoosingBlock;
        private bool isTransitioning;
        private bool isPhaseCompleted;

        private void Start()
        {
            if (phases == null || phases.Count == 0)
            {
                Debug.LogError("[CameraFix] Список фаз пуст!", this);
                return;
            }

            for (int i = 0; i < phases.Count; i++)
            {
                if (phases[i] == null)
                {
                    Debug.LogError(
                        $"[CameraFix] Фаза с индексом {i} не назначена!",
                        this);
                    return;
                }
            }

            CreateSelectionFrame();

            if (selectionFrameInstance == null)
                return;

            // При запуске включаем первую фазу и выключаем остальные.
            for (int i = 0; i < phases.Count; i++)
            {
                phases[i].SetPhaseActive(i == 0);
            }

            currentPhaseIndex = 0;

            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 0f;
                transitionCanvasGroup.blocksRaycasts = false;
                transitionCanvasGroup.interactable = false;
            }

            InitializeCurrentPhase();
        }

        private void Update()
        {
            if (isTransitioning || isPhaseCompleted)
                return;

            if (Keyboard.current == null)
                return;

            if (navigationOrder.Count == 0)
                return;

            if (isChoosingBlock)
            {
                HandleInventoryInput();
                return;
            }

            HandleNavigation();

            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                HandleEnter();
            }

            if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                RemoveSelectedBlock();
            }
        }

        private void InitializeCurrentPhase()
        {
            CameraFixPhaseSetup setup = phases[currentPhaseIndex];

            navigationOrder.Clear();
            availableBlockPrefabs.Clear();
            inventoryDisplayBlocks.Clear();
            originalSprites.Clear();

            selectedIndex = -1;
            selectedInventoryIndex = 0;
            isChoosingBlock = false;
            isPhaseCompleted = false;

            navigationOrder.AddRange(setup.NavigationOrder);
            availableBlockPrefabs.AddRange(setup.AvailableBlockPrefabs);
            inventoryDisplayBlocks.AddRange(setup.InventoryDisplayBlocks);

            if (navigationOrder.Count == 0)
            {
                Debug.LogError(
                    $"[CameraFix] У фазы {currentPhaseIndex + 1} пустой Navigation Order!",
                    setup);
                return;
            }

            if (availableBlockPrefabs.Count != inventoryDisplayBlocks.Count)
            {
                Debug.LogError(
                    $"[CameraFix] Фаза {currentPhaseIndex + 1}: " +
                    "количество префабов и отображаемых блоков не совпадает!",
                    setup);
                return;
            }

            foreach (CircuitBlock block in navigationOrder)
            {
                if (block == null)
                {
                    Debug.LogError(
                        "[CameraFix] В Navigation Order есть пустой элемент!",
                        setup);
                    continue;
                }

                SpriteRenderer renderer = block.GetComponent<SpriteRenderer>();

                if (renderer == null)
                {
                    Debug.LogError(
                        $"[CameraFix] У блока {block.name} отсутствует SpriteRenderer!",
                        block);
                    continue;
                }

                originalSprites[block] = renderer.sprite;
            }

            Debug.Log(
                $"[CameraFix] Активирована фаза {currentPhaseIndex + 1}. " +
                $"Целевое напряжение: {setup.TargetVoltage}.",
                setup);

            SelectFirstEmptySlot();
        }

        private void CreateSelectionFrame()
        {
            if (selectionFramePrefab == null)
            {
                Debug.LogError(
                    "[CameraFix] Не назначен Selection Frame Prefab!",
                    this);
                return;
            }

            selectionFrameInstance = Instantiate(selectionFramePrefab);
            selectionFrameInstance.name = "SelectionFrame_Runtime";

            selectionFrameRenderer =
                selectionFrameInstance.GetComponent<SpriteRenderer>();

            if (selectionFrameRenderer == null)
            {
                Debug.LogError(
                    "[CameraFix] У префаба рамки отсутствует SpriteRenderer!",
                    selectionFrameInstance);

                Destroy(selectionFrameInstance);
                selectionFrameInstance = null;
                return;
            }

            selectionFrameInstance.SetActive(false);
        }

        private void SelectFirstEmptySlot()
        {
            for (int i = 0; i < navigationOrder.Count; i++)
            {
                CircuitBlock block = navigationOrder[i];

                if (block != null && block.IsEmpty)
                {
                    SelectBlock(i);
                    return;
                }
            }

            SelectBlock(0);
        }

        private void HandleNavigation()
        {
            int direction = 0;

            if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                direction = 1;
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                     Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                direction = -1;
            }

            if (direction == 0)
                return;

            int nextIndex = selectedIndex + direction;

            if (nextIndex < 0 || nextIndex >= navigationOrder.Count)
                return;

            SelectBlock(nextIndex);
        }

        private void SelectBlock(int index)
        {
            if (index < 0 || index >= navigationOrder.Count)
                return;

            CircuitBlock block = navigationOrder[index];

            if (block == null)
                return;

            selectedIndex = index;
            MoveSelectionFrameTo(block);
        }

        private void HandleEnter()
        {
            if (selectedIndex < 0 ||
                selectedIndex >= navigationOrder.Count)
                return;

            CircuitBlock target = navigationOrder[selectedIndex];

            if (target == null)
                return;

            if (target.IsFixed && !target.IsEmpty)
            {
                Debug.Log(
                    $"[CameraFix] Блок {target.name} нельзя менять.",
                    target);
                return;
            }

            if (!target.IsEmpty && !target.CanBeRemoved)
            {
                Debug.Log(
                    $"[CameraFix] Блок {target.name} нельзя менять.",
                    target);
                return;
            }

            if (availableBlockPrefabs.Count == 0 ||
                inventoryDisplayBlocks.Count != availableBlockPrefabs.Count)
            {
                Debug.LogError(
                    "[CameraFix] Инвентарь текущей фазы настроен неправильно!",
                    this);
                return;
            }

            isChoosingBlock = true;
            selectedInventoryIndex = 0;

            SelectInventoryBlock(selectedInventoryIndex);
        }

        private void HandleInventoryInput()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                isChoosingBlock = false;
                SelectBlock(selectedIndex);
                return;
            }

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if (selectedInventoryIndex > 0)
                    SelectInventoryBlock(selectedInventoryIndex - 1);
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                if (selectedInventoryIndex < inventoryDisplayBlocks.Count - 1)
                    SelectInventoryBlock(selectedInventoryIndex + 1);
            }

            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                InstallSelectedBlock();
            }
        }

        private void SelectInventoryBlock(int index)
        {
            if (index < 0 || index >= inventoryDisplayBlocks.Count)
                return;

            CircuitBlock displayBlock = inventoryDisplayBlocks[index];

            if (displayBlock == null)
            {
                Debug.LogError(
                    $"[CameraFix] Не назначен блок инвентаря {index}!",
                    this);
                return;
            }

            selectedInventoryIndex = index;
            MoveSelectionFrameTo(displayBlock);
        }

        private void InstallSelectedBlock()
        {
            if (selectedIndex < 0 ||
                selectedIndex >= navigationOrder.Count ||
                selectedInventoryIndex < 0 ||
                selectedInventoryIndex >= availableBlockPrefabs.Count)
                return;

            CircuitBlock target = navigationOrder[selectedIndex];
            CircuitBlock prefab = availableBlockPrefabs[selectedInventoryIndex];

            if (target == null || prefab == null)
                return;

            if (target.IsFixed && !target.IsEmpty)
                return;

            SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
            SpriteRenderer prefabRenderer = prefab.GetComponent<SpriteRenderer>();

            if (targetRenderer == null || prefabRenderer == null)
            {
                Debug.LogError(
                    "[CameraFix] У блока или префаба отсутствует SpriteRenderer!",
                    this);
                return;
            }

            targetRenderer.sprite = prefabRenderer.sprite;

            target.SetBlock(
                prefab.Type,
                prefab.Voltage,
                prefab.IsFixed,
                prefab.CanBeRemoved);

            isChoosingBlock = false;
            SelectBlock(selectedIndex);

            Debug.Log(
                $"[CameraFix] Установлен блок {prefab.name}. " +
                $"Напряжение: {target.Voltage}",
                target);

            CheckVictory();
        }

        private void RemoveSelectedBlock()
        {
            if (selectedIndex < 0 ||
                selectedIndex >= navigationOrder.Count)
                return;

            CircuitBlock block = navigationOrder[selectedIndex];

            if (block == null || block.IsEmpty)
                return;

            if (block.IsFixed || !block.CanBeRemoved)
            {
                Debug.Log(
                    $"[CameraFix] Блок {block.name} нельзя удалить.",
                    block);
                return;
            }

            SpriteRenderer renderer = block.GetComponent<SpriteRenderer>();

            if (renderer == null ||
                !originalSprites.TryGetValue(block, out Sprite originalSprite))
                return;

            renderer.sprite = originalSprite;

            block.SetBlock(
                CircuitBlockType.Empty,
                0,
                false,
                false);

            SelectBlock(selectedIndex);

            Debug.Log($"[CameraFix] Блок {block.name} удалён.", block);

            CheckVictory();
        }

        private void CheckVictory()
        {
            if (isPhaseCompleted)
                return;

            int totalVoltage = 0;

            foreach (CircuitBlock block in navigationOrder)
            {
                if (block != null)
                    totalVoltage += block.Voltage;
            }

            int targetVoltage = phases[currentPhaseIndex].TargetVoltage;

            Debug.Log(
                $"[CameraFix] Напряжение: {totalVoltage}/{targetVoltage}",
                this);

            if (totalVoltage != targetVoltage)
                return;

            isPhaseCompleted = true;

            Debug.Log(
                $"[CameraFix] Фаза {currentPhaseIndex + 1} пройдена!",
                this);

            if (currentPhaseIndex < phases.Count - 1)
            {
                StartCoroutine(TransitionToNextPhase());
            }
            else
            {
                Debug.Log(
                    "[CameraFix] Все три фазы пройдены! Мини-игра завершена.",
                    this);
            }
        }

        private IEnumerator TransitionToNextPhase()
        {
            isTransitioning = true;
            isChoosingBlock = false;

            if (transitionCanvasGroup == null)
            {
                Debug.LogError(
                    "[CameraFix] Не назначен Transition Canvas Group!",
                    this);

                SwitchToNextPhase();
                isTransitioning = false;
                yield break;
            }

            transitionCanvasGroup.blocksRaycasts = true;

            // Затемняем экран.
            yield return FadeTo(1f);

            // Переключаем карту и инвентарь под чёрным экраном.
            SwitchToNextPhase();

            // Показываем новую фазу.
            yield return FadeTo(0f);

            transitionCanvasGroup.blocksRaycasts = false;
            isTransitioning = false;
        }

        private void SwitchToNextPhase()
        {
            phases[currentPhaseIndex].SetPhaseActive(false);

            currentPhaseIndex++;

            phases[currentPhaseIndex].SetPhaseActive(true);

            InitializeCurrentPhase();

            Debug.Log(
                $"[CameraFix] Переход на фазу {currentPhaseIndex + 1}.",
                this);
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (transitionCanvasGroup == null)
                yield break;

            float startAlpha = transitionCanvasGroup.alpha;

            if (fadeDuration <= 0f)
            {
                transitionCanvasGroup.alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / fadeDuration);

                transitionCanvasGroup.alpha =
                    Mathf.Lerp(startAlpha, targetAlpha, progress);

                yield return null;
            }

            transitionCanvasGroup.alpha = targetAlpha;
        }

        private void MoveSelectionFrameTo(CircuitBlock block)
        {
            if (block == null ||
                selectionFrameInstance == null ||
                selectionFrameRenderer == null)
                return;

            SpriteRenderer blockRenderer = block.GetComponent<SpriteRenderer>();

            if (blockRenderer == null)
                return;

            Sprite frameSprite = selectionFrameRenderer.sprite;

            if (frameSprite == null)
                return;

            Bounds bounds = blockRenderer.bounds;
            Vector3 spriteSize = frameSprite.bounds.size;

            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
                return;

            selectionFrameInstance.transform.position = new Vector3(
                bounds.center.x,
                bounds.center.y,
                block.transform.position.z - 0.1f);

            selectionFrameInstance.transform.localScale = new Vector3(
                bounds.size.x / spriteSize.x,
                bounds.size.y / spriteSize.y,
                1f);

            selectionFrameInstance.SetActive(true);
        }
    }
}
