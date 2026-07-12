using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration.Save
{
    [RequireComponent(typeof(Collider2D), typeof(CheckpointSaveFeedbackView))]
    public sealed class ExplorationCheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private string checkpointId;
        [SerializeField] private LayerMask playerLayers = 1;
        [SerializeField] private CheckpointTriggerRepeatMode repeatMode;
        [SerializeField, Min(0f)] private float cooldownSeconds = 1f;

        private readonly HashSet<int> playerColliderIds = new();

        private Collider2D triggerCollider;
        private CheckpointSaveFeedbackView feedbackView;
        private ExplorationCheckpointSaveService checkpointSaveService;
        private IRuntimeErrorLogger errorLogger;
        private bool isConfigurationValid;
        private bool isArmed = true;
        private bool isSaveInProgress;
        private float nextAllowedTriggerTime;

        [Inject]
        public void Construct(
            ExplorationCheckpointSaveService checkpointSaveService,
            IRuntimeErrorLogger errorLogger)
        {
            this.checkpointSaveService = checkpointSaveService;
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isConfigurationValid || !IsPlayerLayer(other.gameObject.layer))
                return;

            bool isFirstPlayerCollider = playerColliderIds.Count == 0;
            playerColliderIds.Add(other.GetInstanceID());

            if (!isFirstPlayerCollider || isSaveInProgress || !CanTrigger())
                return;

            isArmed = false;
            nextAllowedTriggerTime = Time.unscaledTime + cooldownSeconds;
            SaveCheckpointAsync(destroyCancellationToken).Forget();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayerLayer(other.gameObject.layer))
                return;

            playerColliderIds.Remove(other.GetInstanceID());

            if (playerColliderIds.Count == 0 && repeatMode == CheckpointTriggerRepeatMode.UntilPlayerExit)
                isArmed = true;
        }

        private bool CanTrigger()
        {
            return repeatMode == CheckpointTriggerRepeatMode.UntilPlayerExit
                ? isArmed
                : Time.unscaledTime >= nextAllowedTriggerTime;
        }

        private bool IsPlayerLayer(int layer)
        {
            return (playerLayers.value & (1 << layer)) != 0;
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
            else if (playerLayers.value == 0)
                validationError = "Checkpoint player layer mask is not configured.";
            else if (feedbackView == null)
                validationError = "Checkpoint save feedback view is required.";
            else if (checkpointSaveService == null || !checkpointSaveService.IsCoordinatorRegistered)
                validationError = "GameSaveCoordinator is not registered for checkpoint saving.";

            if (validationError == null)
                return true;

            errorLogger.LogException(
                new InvalidOperationException(validationError),
                $"Checkpoint validation failed: {checkpointId}");
            return false;
        }

        private async UniTask SaveCheckpointAsync(CancellationToken cancellationToken)
        {
            isSaveInProgress = true;

            try
            {
                await checkpointSaveService.SaveCheckpointAsync(checkpointId, cancellationToken);
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
    }
}
