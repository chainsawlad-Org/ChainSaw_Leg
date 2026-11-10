using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public sealed class MonsterAttack : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0f)] private float damageCooldown = 1f;

        [Header("Touch Detection")]
        [SerializeField] private Collider2D touchCollider;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField, Min(0f)] private float touchPadding = 0.15f;

        [Header("References")]
        [SerializeField] private MonsterHealth monsterHealth;

        private float nextDamageTime = -Mathf.Infinity;

        private void Awake()
        {
            if (touchCollider == null) touchCollider = GetComponent<Collider2D>();
            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();
        }

        private void Update()
        {
            if (Time.time < nextDamageTime) return;
            if (monsterHealth != null && !monsterHealth.IsAlive) return;

            Vector2 center = touchCollider != null
                ? (Vector2)touchCollider.bounds.center
                : (Vector2)transform.position;

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, GetTouchRadius(), targetMask);

            foreach (Collider2D hit in hits)
            {
                PlatformerPlayerHealth playerHealth = hit.GetComponentInParent<PlatformerPlayerHealth>();

                if (playerHealth == null || !playerHealth.IsAlive || playerHealth.IsInvulnerable) continue;

                nextDamageTime = Time.time + damageCooldown;
                playerHealth.TakeDamage(damage);

                return;
            }
        }

        private float GetTouchRadius()
        {
            if (touchCollider == null) return touchPadding;

            Vector2 extents = touchCollider.bounds.extents;

            return Mathf.Max(extents.x, extents.y) + touchPadding;
        }

        private void OnDrawGizmosSelected()
        {
            if (touchCollider == null) touchCollider = GetComponent<Collider2D>();

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(touchCollider != null ? touchCollider.bounds.center : transform.position, GetTouchRadius());
        }
    }
}
