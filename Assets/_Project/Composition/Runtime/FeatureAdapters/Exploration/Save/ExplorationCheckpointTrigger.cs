using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration.Save
{
    [RequireComponent(typeof(Collider2D), typeof(CheckpointSaveFeedbackView))]
    public sealed class ExplorationCheckpointTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string checkpointId;
        [SerializeField] private string interactionPrompt = "Нажмите E для сохранения";

        private Collider2D triggerCollider;
        private CheckpointSaveFeedbackView feedbackView;
        private CheckpointSaveRequestBroker saveRequestBroker;
        private IRuntimeErrorLogger errorLogger;
        private bool isConfigurationValid;
        private bool isSaveInProgress;

        [Inject]
        public void Construct(
            CheckpointSaveRequestBroker saveRequestBroker,
            IRuntimeErrorLogger errorLogger)
        {
            this.saveRequestBroker = saveRequestBroker;
            this.errorLogger = errorLogger;
        }

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            feedbackView = GetComponent<CheckpointSaveFeedbackView>();
        }

        private void Start()
        {
            isConfigurationValid = ValidateConfiguration();
            enabled = isConfigurationValid;
        }

        public string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        public bool CanInteract()
        {
            return isConfigurationValid && !isSaveInProgress;
        }

        public void Interact()
        {
            if (!CanInteract())
                return;

            InteractAsync(destroyCancellationToken).Forget();
        }

        private async UniTask InteractAsync(CancellationToken cancellationToken)
        {
            isSaveInProgress = true;

            try
            {
                bool saved = await saveRequestBroker.RequestSaveAsync(checkpointId, cancellationToken);

                if (saved)
                    feedbackView.ShowAsync(cancellationToken).Forget();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                errorLogger.LogException(exception, $"Checkpoint save failed: {checkpointId}");
            }
            finally
            {
                isSaveInProgress = false;
            }
        }

        private bool ValidateConfiguration()
        {
            string validationError = null;

            if (string.IsNullOrWhiteSpace(checkpointId))
                validationError = "Checkpoint ID is required.";
            else if (triggerCollider == null)
                validationError = "Checkpoint Collider2D is required.";
            else if (!triggerCollider.isTrigger)
                validationError = "Checkpoint Collider2D must be configured as a trigger.";
            else if (feedbackView == null)
                validationError = "Checkpoint save feedback view is required.";
            else if (saveRequestBroker == null)
                validationError = "CheckpointSaveRequestBroker is not registered for checkpoint saving.";

            if (validationError == null)
                return true;

            errorLogger.LogException(
                new InvalidOperationException(validationError),
                $"Checkpoint validation failed: {checkpointId}");
            return false;
        }
    }
}
