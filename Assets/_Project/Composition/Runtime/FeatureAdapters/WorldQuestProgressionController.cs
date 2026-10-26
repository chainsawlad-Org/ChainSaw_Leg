using ChainSawLeg.Features.Dialogue;
using ChainSawLeg.Features.Quest;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public sealed class WorldQuestProgressionController : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private string requiredDialogueId;
        [SerializeField] private string questId;

        private IDialogueRunner dialogueRunner;
        private MissionProgressionService questService;
        private Collider2D wallCollider;
        
        private bool isOpened;

        [Inject]
        private void Construct(
            IDialogueRunner dialogueRunner,
            MissionProgressionService questService)
        {
            this.dialogueRunner = dialogueRunner;
            this.questService = questService;

            this.dialogueRunner.DialogueEnded += HandleDialogueEnded;
            
            Debug.Log(
                "[WorldQuestProgressionController] Injected. " +
                $"Required dialogue: '{requiredDialogueId}', " +
                $"Quest: '{questId}'.",
                this);
        }

        private void Awake()
        {
            wallCollider = GetComponent<Collider2D>();

            if (wallCollider == null)
            {
                Debug.LogError(
                    "[WorldQuestProgressionController] " +
                    "No Collider2D found on InvisibleWall.",
                    this);
            }
        }

        private void OnDestroy()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.DialogueEnded -= HandleDialogueEnded;
            }
        }

        private void HandleDialogueEnded(Dialogue dialogue)
        {
            Debug.Log(
                "[WorldQuestProgressionController] DialogueEnded event received.",
                this);
            
            if (isOpened || dialogue == null)
            {
                Debug.LogWarning(
                    "[WorldQuestProgressionController] Dialogue is null.",
                    this);
                return;
            }
            
            Debug.Log(
                $"[WorldQuestProgressionController] " +
                $"Ended dialogue ID: '{dialogue.Id}'. " +
                $"Expected ID: '{requiredDialogueId}'.",
                this);
            
            if (isOpened)
            {
                Debug.Log("[WorldQuestProgressionController] Passage is already open.", this);
                return;
            }

            if (dialogue.Id != requiredDialogueId)
            {
                Debug.Log(
                    "[WorldQuestProgressionController] Dialogue ID does not match.",
                    this);
                return;
            }

            QuestStatus status = questService.GetStatus(questId);
            
            Debug.Log(
                $"[WorldQuestProgressionController] Quest '{questId}' status: {status}.",
                this);

            if (status != QuestStatus.InProgress)
            {
                Debug.LogWarning(
                    $"[WorldQuestProgressionController] " +
                    $"Quest '{questId}' is not InProgress. " +
                    $"Current status: {status}.",
                    this);

                return;
            }

            bool completed = questService.CompleteQuest(questId);
            
            Debug.Log(
                $"[WorldQuestProgressionController] CompleteQuest result: {completed}.",
                this);

            if (!completed)
            {
                Debug.LogWarning(
                    $"[WorldQuestProgressionController] " +
                    $"Could not complete quest '{questId}'.",
                    this);

                return;
            }

            if (wallCollider == null)
            {
                Debug.LogError(
                    "[WorldQuestProgressionController] " +
                    "Cannot open passage because Collider2D is missing.",
                    this);

                return;
            }

            isOpened = true;
            wallCollider.enabled = false;

            Debug.Log(
                "[WorldQuestProgressionController] SUCCESS: passage opened.",
                this);
        }
    }
}
