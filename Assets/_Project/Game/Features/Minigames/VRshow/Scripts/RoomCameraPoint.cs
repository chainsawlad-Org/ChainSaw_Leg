using ChainSawLeg.Features.Exploration;
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

        private CameraFlow cameraFlow;
        private Collider2D triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null) triggerCollider.isTrigger = true;

            cameraFlow = FindFirstObjectByType<CameraFlow>();
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

        private void OnDrawGizmosSelected()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
