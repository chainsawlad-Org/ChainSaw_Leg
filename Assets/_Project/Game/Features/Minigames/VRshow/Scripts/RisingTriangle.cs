using ChainSawLeg.Features.Exploration;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// A kamikaze hazard: rises straight up, damages the player once on contact
    /// and then self-destructs. It flies through everything: movement is
    /// transform-based and player detection is a physics query, so it needs
    /// (and has) no colliders of its own. Optionally loops: hides after
    /// maxTravelHeight, then respawns at its start.
    /// </summary>
    public sealed class RisingTriangle : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float flySpeed = 6f;
        [Tooltip("How far above the start position it flies before hiding. 0 = fly forever.")]
        [SerializeField, Min(0f)] private float maxTravelHeight = 25f;
        [SerializeField, Min(0f)] private float respawnDelay = 2f;
        [SerializeField, Min(0f)] private float spinSpeed = 0f;

        [Header("Damage")]
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0f)] private float damageRadius = 0.6f;
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer visual;

        private Vector3 startPosition;
        private float respawnTimer;

        private void Awake()
        {
            startPosition = transform.position;
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (visual != null && visual.sprite == null) visual.sprite = CreateTriangleSprite();
        }

        private void Update()
        {
            // Waiting to respawn: invisible until the timer ends, then back to start.
            if (respawnTimer > 0f)
            {
                respawnTimer -= Time.deltaTime;

                if (respawnTimer <= 0f)
                {
                    transform.position = startPosition;
                    if (visual != null) visual.enabled = true;
                }

                return;
            }

            transform.position += Vector3.up * (flySpeed * Time.deltaTime);

            if (spinSpeed > 0f)
                transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            float traveled = transform.position.y - startPosition.y;
            if (maxTravelHeight > 0f && traveled >= maxTravelHeight)
            {
                respawnTimer = respawnDelay;
                if (visual != null) visual.enabled = false;
                return;
            }

            TryDamagePlayer();
        }

        public void TakeDamage(int amount)
        {
            Destroy(gameObject);
        }

        private void TryDamagePlayer()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, targetMask);

            foreach (Collider2D hit in hits)
            {
                PlatformerPlayerHealth playerHealth = hit.GetComponentInParent<PlatformerPlayerHealth>();

                // Invulnerable player: fly on, don't sacrifice the triangle for nothing.
                if (playerHealth == null || !playerHealth.IsAlive || playerHealth.IsInvulnerable) continue;

                playerHealth.TakeDamage(damage, transform.position);

                // One hit is all it takes: the triangle goes with it.
                Destroy(gameObject);

                return;
            }
        }

        private static Sprite CreateTriangleSprite()
        {
            const int size = 64;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };

            Vector2 apex = new Vector2(size * 0.5f, size - 2f);
            Vector2 left = new Vector2(2f, 2f);
            Vector2 right = new Vector2(size - 2f, 2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float a = InsideTriangle(p, apex, left, right) ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }

            texture.Apply();

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private static bool InsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;

            return !(hasNeg && hasPos);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadius);

            if (maxTravelHeight > 0f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
                Gizmos.DrawLine(transform.position, transform.position + Vector3.up * maxTravelHeight);
            }
        }
    }
}
