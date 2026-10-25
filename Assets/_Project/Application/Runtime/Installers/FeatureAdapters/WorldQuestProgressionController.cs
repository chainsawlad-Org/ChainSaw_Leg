using ChainSawLeg.Features.Dialogue;
using ChainSawLeg.Features.Quest;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public sealed class WorldQuestProgressionController : MonoBehaviour
    {
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
            if (isOpened || dialogue == null)
            {
                return;
            }

            if (dialogue.Id != requiredDialogueId)
            {
                return;
            }

            QuestStatus status = questService.GetStatus(questId);

            if (status == QuestStatus.Available)
            {
                questService.StartQuest(questId);
                status = questService.GetStatus(questId);
            }

            if (status != QuestStatus.InProgress)
            {
                Debug.LogWarning(
                    $"[WorldQuestProgressionController] " +
                    $"Quest '{questId}' cannot be completed. " +
                    $"Current status: {status}.",
                    this);

                return;
            }

            questService.CompleteQuest(questId);

            if (questService.GetStatus(questId) != QuestStatus.Completed)
            {
                Debug.LogWarning(
                    $"[WorldQuestProgressionController] " +
                    $"Quest '{questId}' was not completed.",
                    this);

                return;
            }

            isOpened = true;

            if (wallCollider != null)
            {
                wallCollider.enabled = false;
            }

            Debug.Log(
                $"[WorldQuestProgressionController] " +
                $"Quest '{questId}' completed. InvisibleWall opened.",
                this);
        }
    }
}
