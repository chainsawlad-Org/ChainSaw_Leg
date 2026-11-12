using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Holds the last reached checkpoint position. Statics survive a scene load,
    /// so the respawn point carries over when the minigame scene restarts.
    /// </summary>
    public static class PlatformerCheckpointData
    {
        public static bool HasPosition { get; private set; }
        public static Vector3 Position { get; private set; }

        public static void Save(Vector3 position)
        {
            Position = position;
            HasPosition = true;
        }

        public static void Clear()
        {
            HasPosition = false;
        }

        // Keeps play sessions independent even if "Enter Play Mode Options" disables domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Clear();
        }
    }

    public sealed class CheckPoint : MonoBehaviour
    {
        [Header("Touch Detection")]
        [SerializeField] private Collider2D touchCollider;
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Feedback")]
        [SerializeField] private Color activatedColor = new Color(0.4f, 0.9f, 0.4f);
        [SerializeField] private SpriteRenderer visual;

        private bool activated;

        private void Awake()
        {
            if (touchCollider == null) touchCollider = GetComponent<Collider2D>();
            if (visual == null) visual = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleTouch(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleTouch(collision.collider);
        }

        private void HandleTouch(Collider2D other)
        {
            if (activated) return;

            PlatformerPlayerHealth playerHealth = other.GetComponentInParent<PlatformerPlayerHealth>();

            if (playerHealth == null || !playerHealth.IsAlive) return;

            playerHealth.Heal(99);
            activated = true;
            PlatformerCheckpointData.Save(playerHealth.transform.position);

            AudioService.Play(AudioId.Heal);

            if (visual != null) visual.color = activatedColor;
        }

        private void OnDrawGizmosSelected()
        {
            if (touchCollider == null) touchCollider = GetComponent<Collider2D>();
            if (touchCollider == null) return;

            Gizmos.color = activated ? Color.green : Color.cyan;
            Gizmos.DrawWireCube(touchCollider.bounds.center, touchCollider.bounds.size);
        }
    }
}
