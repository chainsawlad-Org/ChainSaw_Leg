using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public sealed class Spikes : MonoBehaviour
    {
        [Header("Touch Detection")]
        [SerializeField] private Collider2D killCollider;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField, Min(0f)] private float touchPadding = 0.05f;

        private void Awake()
        {
            if (killCollider == null) killCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (killCollider == null) return;

            Bounds bounds = killCollider.bounds;
            Vector2 center = bounds.center;
            Vector2 size = (Vector2)bounds.size + new Vector2(touchPadding * 2f, touchPadding * 2f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, targetMask);

            foreach (Collider2D hit in hits)
            {
                PlatformerPlayerHealth playerHealth = hit.GetComponentInParent<PlatformerPlayerHealth>();

                if (playerHealth == null || !playerHealth.IsAlive) continue;

                playerHealth.Kill();

                return;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (killCollider == null) killCollider = GetComponent<Collider2D>();
            if (killCollider == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(killCollider.bounds.center, killCollider.bounds.size);
        }
    }
}
