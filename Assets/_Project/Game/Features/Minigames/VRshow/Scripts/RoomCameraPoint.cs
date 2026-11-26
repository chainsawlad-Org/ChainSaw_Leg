using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// A room-sized trigger that bounds the camera: when the player enters it,
    /// the camera's follow bounds switch to this trigger's own bounds and the
    /// camera smoothly transitions into the room.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class RoomCameraPoint : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float transitionDuration = 0.5f;
        [SerializeField] private Transform[] platformsToNextRoom;

        private CameraFlow cameraFlow;
        private Collider2D triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null) triggerCollider.isTrigger = true;

            cameraFlow = FindFirstObjectByType<CameraFlow>();
            foreach (var platform in platformsToNextRoom) platform.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (cameraFlow == null || triggerCollider == null) return;

            PlatformerPlayerHealth playerHealth = other.GetComponentInParent<PlatformerPlayerHealth>();

            if (playerHealth == null || !playerHealth.IsAlive) return;

            Bounds roomBounds = triggerCollider.bounds;
            cameraFlow.bounds = roomBounds;
            cameraFlow.bounds.min = triggerCollider.transform.position;
            cameraFlow.bounds.max = triggerCollider.transform.position;
            cameraFlow.clampPosition = true;
            cameraFlow.TransitToRoom(roomBounds, transitionDuration, playerHealth.transform.position);
        }

        public void ActivatePlatformsToNextRoom()
        {
            foreach (var platform in platformsToNextRoom)
            {
                platform.gameObject.SetActive(true);
                Vector3 scale = platform.localScale;
                platform.DOScale(scale, 1f).From(Vector3.zero);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
