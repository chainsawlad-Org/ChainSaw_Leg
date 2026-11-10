using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Boss brain for EvilMisty. Runs an attack loop: telegraphs and performs one
    /// attack at a time, then idles. New attacks are added as coroutines next to
    /// HurricaneAttack and scheduled in RunAttackLoop.
    /// </summary>
    public sealed class EvilMistyBoss : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Behavior")]
        [SerializeField, Min(0f)] private float firstAttackDelay = 1f;
        [SerializeField, Min(0f)] private float idleTimeBetweenAttacks = 2f;

        [Header("Hurricane Attack")]
        [SerializeField, Min(1)] private int dashCount = 3;
        [SerializeField, Min(0f)] private float dashSpeed = 15f;
        [SerializeField, Min(0f)] private float dashDuration = 0.6f;
        [SerializeField, Min(0f)] private float pauseBetweenDashes = 0.35f;
        [SerializeField, Min(0f)] private float telegraphDuration = 0.5f;
        [SerializeField, Min(0f)] private float telegraphShakeStrength = 0.25f;
        [SerializeField, Min(0)] private int telegraphShakeVibrato = 40;

        [Header("Wall Detection")]
        [SerializeField] private LayerMask wallMask = ~0;
        [SerializeField, Min(0f)] private float wallCheckDistance = 0.3f;

        [Header("Touch Damage")]
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField] private Collider2D damageCollider;
        [SerializeField, Min(0f)] private float touchPadding = 0.15f;
        [SerializeField, Min(0f)] private float damageCooldown = 0.5f;

        [Header("Visuals")]
        [SerializeField] private Transform spinningVisual;
        [SerializeField, Min(0f)] private float spinSpeed = 720f;
        [SerializeField] private Sprite hurricaneSprite;

        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private Sprite defaultSprite;
        private float defaultGravityScale = 1f;
        private PlatformerPlayerHealth playerHealth;
        private float nextDamageTime = -Mathf.Infinity;
        private bool isDashing;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (body == null) Debug.LogWarning("EvilMistyBoss needs a Rigidbody2D to dash.", this);

            if (damageCollider == null) damageCollider = GetComponent<Collider2D>();
            if (spinningVisual == null)
            {
                SpriteRenderer childSprite = GetComponentInChildren<SpriteRenderer>();
                if (childSprite != null && childSprite.transform != transform) spinningVisual = childSprite.transform;
            }

            sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite != null) defaultSprite = sprite.sprite;

            if (body != null) defaultGravityScale = body.gravityScale;
        }

        private void Start()
        {
            playerHealth = FindFirstObjectByType<PlatformerPlayerHealth>();
            StartCoroutine(RunAttackLoop());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            if (isDashing && spinningVisual != null)
                spinningVisual.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            TryDamagePlayer();
        }

        private IEnumerator RunAttackLoop()
        {
            yield return new WaitForSeconds(firstAttackDelay);

            while (true)
            {
                if (playerHealth != null && playerHealth.IsAlive)
                    yield return HurricaneAttack();

                yield return new WaitForSeconds(idleTimeBetweenAttacks);
            }
        }

        private IEnumerator HurricaneAttack()
        {
            // Telegraph: stop and shake, then dash.
            if (body != null) body.linearVelocity = Vector2.zero;
            if (telegraphDuration > 0f)
                transform.DOShakePosition(telegraphDuration, telegraphShakeStrength, telegraphShakeVibrato);
            yield return new WaitForSeconds(telegraphDuration);

            if (body == null || playerHealth == null || !playerHealth.IsAlive) yield break;

            body.gravityScale = 0f;
            if (sprite != null && hurricaneSprite != null) sprite.sprite = hurricaneSprite;

            for (int i = 0; i < dashCount; i++)
            {
                if (!playerHealth.IsAlive) break;

                // Dash along the horizontal axis only: chase the player's side, keep Y locked.
                float directionX = Mathf.Sign(playerHealth.transform.position.x - transform.position.x);
                if (directionX == 0f) directionX = 1f;
                body.linearVelocity = new Vector2(directionX * dashSpeed, 0f);

                isDashing = true;
                float elapsed = 0f;
                while (elapsed < dashDuration && playerHealth.IsAlive)
                {
                    elapsed += Time.deltaTime;
                    body.linearVelocityY = 0f;
                    TryDamagePlayer();

                    // Stop immediately when a wall is ahead.
                    if (IsWallAhead(directionX)) break;

                    yield return null;
                }
                isDashing = false;

                body.linearVelocity = Vector2.zero;

                if (!playerHealth.IsAlive) break;

                yield return new WaitForSeconds(pauseBetweenDashes);
            }

            body.gravityScale = defaultGravityScale;
            if (sprite != null && defaultSprite != null) sprite.sprite = defaultSprite;
        }

        private bool IsWallAhead(float directionX)
        {
            if (damageCollider == null) return false;

            Vector2 direction = new Vector2(directionX, 0f);
            Bounds bounds = damageCollider.bounds;
            Vector2 origin = (Vector2)bounds.center + direction * (bounds.extents.x + 0.02f);

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, wallCheckDistance, wallMask);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == damageCollider) continue;
                if (hit.collider.GetComponentInParent<PlatformerPlayerHealth>() != null) continue;

                return true;
            }

            return false;
        }

        private void TryDamagePlayer()
        {
            if (Time.time < nextDamageTime || damageCollider == null) return;

            Bounds bounds = damageCollider.bounds;
            Vector2 center = bounds.center;
            Vector2 size = (Vector2)bounds.size + new Vector2(touchPadding * 2f, touchPadding * 2f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, targetMask);

            foreach (Collider2D hit in hits)
            {
                PlatformerPlayerHealth hitHealth = hit.GetComponentInParent<PlatformerPlayerHealth>();

                if (hitHealth == null || !hitHealth.IsAlive) continue;

                nextDamageTime = Time.time + damageCooldown;
                hitHealth.TakeDamage(damage);

                return;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (damageCollider == null) damageCollider = GetComponent<Collider2D>();
            if (damageCollider == null) return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(damageCollider.bounds.center, damageCollider.bounds.size);
        }
    }
}
