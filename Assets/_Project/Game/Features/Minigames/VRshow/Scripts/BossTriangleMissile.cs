using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// A boss-spawned triangle that flies straight in a configured direction,
    /// damages the player once and self-destructs. The player's attack can also
    /// destroy it mid-flight. Trigger collider only: it never physically blocks
    /// anything.
    /// </summary>
    public sealed class BossTriangleMissile : MonoBehaviour, IDamageable
    {
        [Header("Flight")]
        [SerializeField, Min(0.01f)] private float flySpeed = 8f;
        [SerializeField, Min(0.01f)] private float lifetime = 4f;

        [Header("Damage")]
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0f)] private float damageRadius = 0.5f;
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer visual;

        private Vector2 flyDirection = Vector2.up;

        private void Awake()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (visual != null && visual.sprite == null) visual.sprite = CreateTriangleSprite();
            if (visual != null && visual.color == Color.white) visual.color = new Color(0.9f, 0.25f, 0.2f, 1f);

            // Trigger-only collider: lets the player's attack find the missile
            // while it still flies through every obstacle.
            if (GetComponent<Collider2D>() == null)
            {
                BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = Vector2.one;
            }

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += (Vector3)(flyDirection * (flySpeed * Time.deltaTime));

            TryDamagePlayer();
        }

        private void TryDamagePlayer()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, targetMask);

            foreach (Collider2D hit in hits)
            {
                PlatformerPlayerHealth playerHealth = hit.GetComponentInParent<PlatformerPlayerHealth>();

                // Invulnerable player: fly on, don't waste the missile.
                if (playerHealth == null || !playerHealth.IsAlive || playerHealth.IsInvulnerable) continue;

                playerHealth.TakeDamage(damage, transform.position);

                Destroy(gameObject);

                return;
            }
        }

        public void TakeDamage(int amount)
        {
            Destroy(gameObject);
        }

        /// <summary>Applies the boss's volley settings after the component is added.</summary>
        public void Configure(float speed, int damageAmount, float radius, float lifeTime, Vector2 direction)
        {
            flySpeed = speed;
            damage = damageAmount;
            damageRadius = radius;
            lifetime = lifeTime;
            flyDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;

            // The triangle sprite points up, so rotate it to face its flight path.
            transform.rotation = Quaternion.FromToRotation(Vector3.up, flyDirection);

            Destroy(gameObject, lifeTime);
        }

        internal static Sprite CreateTriangleSprite()
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
    }
}
