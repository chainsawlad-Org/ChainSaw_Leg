using System.Collections;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Boss brain for EvilMisty. Runs an attack loop that strictly alternates between
    /// attacks (Hurricane → Slam → Hurricane → …). New attacks are added as coroutines
    /// next to HurricaneAttack/SlamAttack and scheduled in RunAttackLoop.
    /// </summary>
    public sealed class EvilMistyBoss : MonoBehaviour
    {
        public enum BossState { Idle, Telegraph, Jump, Land, Recover }

        [Header("Target")]
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Behavior")]
        [SerializeField, Min(0f)] private float firstAttackDelay = 1f;
        [SerializeField, Min(0f)] private float idleTimeBetweenAttacks = 2f;

        [Header("Telegraph")]
        [SerializeField, Min(0f)] private float telegraphDuration = 0.5f;
        [SerializeField, Min(0f)] private float telegraphShakeStrength = 0.25f;
        [SerializeField, Min(0)] private int telegraphShakeVibrato = 40;

        [Header("Hurricane Attack")]
        [SerializeField, Min(1)] private int dashCount = 3;
        [SerializeField, Min(0f)] private float dashSpeed = 15f;
        [SerializeField, Min(0f)] private float dashDuration = 0.6f;
        [SerializeField, Min(0f)] private float pauseBetweenDashes = 0.35f;

        [Header("Slam Attack")]
        [SerializeField, Min(1)] private int slamBounceCountHealthy = 1;
        [SerializeField, Min(1)] private int slamBounceCountEnraged = 3;
        [SerializeField, Range(0.01f, 1f)] private float enragedHealthFraction = 0.5f;
        [SerializeField, Min(0f)] private float slamArcHeight = 4f;
        [SerializeField, Min(0f)] private float slamJumpDuration = 0.9f;
        [SerializeField, Range(0f, 1f)] private float slamLockAtProgress = 0.7f;
        [SerializeField, Min(0f)] private float slamRecoverDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float slamRadius = 2.5f;

        [Header("Slam Exit Bounce (visual only)")]
        [SerializeField, Min(0f)] private float slamExitBounceDistance = 1.5f;
        [SerializeField, Min(0f)] private float slamExitBounceHeight = 1.6f;
        [SerializeField, Min(0f)] private float slamExitBounceDuration = 0.45f;
        [SerializeField, Min(0f)] private float slamKnockbackSide = 12f;
        [SerializeField, Min(0f)] private float slamKnockbackUp = 18f;
        [SerializeField] private bool showLandingMarker = true;
        [SerializeField, Min(0f)] private float slamCameraShakeStrength = 0.6f;

        [Header("Wall / Ground Detection")]
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
        [SerializeField] private CameraFlow cameraFlow;

        [Header("References")]
        [SerializeField] private MonsterHealth monsterHealth;

        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private Sprite defaultSprite;
        private float defaultGravityScale = 1f;
        private PlatformerPlayerHealth playerHealth;
        private float nextDamageTime = -Mathf.Infinity;
        private bool isDashing;
        private bool useHurricaneNext = true;
        private GameObject landingMarker;
        private SpriteRenderer landingMarkerRenderer;

        public BossState CurrentState { get; private set; } = BossState.Idle;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (body == null) Debug.LogWarning("EvilMistyBoss needs a Rigidbody2D to dash.", this);

            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();

            if (damageCollider == null) damageCollider = GetComponent<Collider2D>();
            if (spinningVisual == null)
            {
                SpriteRenderer childSprite = GetComponentInChildren<SpriteRenderer>();
                if (childSprite != null && childSprite.transform != transform) spinningVisual = childSprite.transform;
            }

            sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite != null) defaultSprite = sprite.sprite;

            if (body != null) defaultGravityScale = body.gravityScale;

            if (cameraFlow == null) cameraFlow = FindFirstObjectByType<CameraFlow>();

            if (showLandingMarker) CreateLandingMarker();
        }

        private void Start()
        {
            playerHealth = FindFirstObjectByType<PlatformerPlayerHealth>();
            StartCoroutine(RunAttackLoop());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();

            if (landingMarker != null) Destroy(landingMarker);
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
                {
                    // Strict alternation: hurricane → slam → hurricane → …
                    yield return useHurricaneNext ? HurricaneAttack() : SlamAttack();
                    useHurricaneNext = !useHurricaneNext;
                }

                CurrentState = BossState.Idle;
                yield return new WaitForSeconds(idleTimeBetweenAttacks);
            }
        }

        private IEnumerator HurricaneAttack()
        {
            // Telegraph: stop and shake, then dash.
            CurrentState = BossState.Telegraph;
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

        private IEnumerator SlamAttack()
        {
            // Telegraph: stop and shake before the first jump.
            CurrentState = BossState.Telegraph;
            if (body != null) body.linearVelocity = Vector2.zero;
            if (telegraphDuration > 0f)
                transform.DOShakePosition(telegraphDuration, telegraphShakeStrength, telegraphShakeVibrato);
            yield return new WaitForSeconds(telegraphDuration);

            if (body == null || playerHealth == null || !playerHealth.IsAlive) yield break;

            // The tween drives the transform, so physics must not fight it mid-air.
            body.gravityScale = 0f;
            body.simulated = false;

            // Distance from the transform origin to the collider bottom while standing.
            float standOffset = damageCollider != null
                ? transform.position.y - damageCollider.bounds.min.y
                : 0f;

            // Difficulty progression: bounces increase once half the boss health is gone.
            int bounceCount = IsEnraged() ? slamBounceCountEnraged : slamBounceCountHealthy;

            for (int i = 0; i < bounceCount; i++)
            {
                if (!playerHealth.IsAlive) break;

                // Arc jump: aim at the player's X at take-off, land on solid ground there.
                CurrentState = BossState.Jump;
                float targetX = playerHealth.transform.position.x;
                FindGroundAt(targetX, transform.position.y, out float groundY);

                Vector3 target = new Vector3(targetX, groundY + standOffset, transform.position.z);
                SetLandingMarker(targetX, groundY, false);

                bool landed = false;
                Tween jumpTween = transform.DOJump(target, slamArcHeight, 1, slamJumpDuration)
                    .SetEase(Ease.Linear);

                // The arc flight is the dodge window; the marker locks near the end.
                jumpTween.OnUpdate(() =>
                {
                    if (jumpTween.ElapsedPercentage() >= slamLockAtProgress)
                        SetLandingMarker(targetX, groundY, true);
                });
                jumpTween.OnComplete(() => landed = true);

                while (!landed && playerHealth.IsAlive)
                    yield return null;

                if (jumpTween.IsActive()) jumpTween.Kill();

                // Land: impact, then bounce straight into the next arc jump.
                CurrentState = BossState.Land;
                if (landingMarker != null) landingMarker.SetActive(false);
                SlamImpact();
            }

            // Cosmetic exit bounce: a small hop away from the player, no hitbox, no marker.
            if (playerHealth.IsAlive)
            {
                CurrentState = BossState.Jump;
                float exitDirectionX = Mathf.Sign(transform.position.x - playerHealth.transform.position.x);
                if (exitDirectionX == 0f) exitDirectionX = 1f;

                float exitX = transform.position.x + exitDirectionX * slamExitBounceDistance;
                FindGroundAt(exitX, transform.position.y, out float exitGroundY);

                Vector3 exitTarget = new Vector3(exitX, exitGroundY + standOffset, transform.position.z);
                transform.DOJump(exitTarget, slamExitBounceHeight, 1, slamExitBounceDuration)
                    .SetEase(Ease.Linear);

                yield return new WaitForSeconds(slamExitBounceDuration);
            }

            body.simulated = true;
            body.linearVelocity = Vector2.zero;
            body.gravityScale = defaultGravityScale;

            // Recover: boss stands still, giving the player a safe window.
            CurrentState = BossState.Recover;
            yield return new WaitForSeconds(slamRecoverDuration);
        }

        private bool IsEnraged()
        {
            return monsterHealth != null
                && monsterHealth.CurrentHealth <= monsterHealth.MaxHealth * enragedHealthFraction;
        }

        private void SlamImpact()
        {
            Vector2 center = damageCollider != null
                ? (Vector2)damageCollider.bounds.center
                : (Vector2)transform.position;

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, slamRadius, targetMask);

            foreach (Collider2D hit in hits)
            {
                PlatformerPlayerHealth hitHealth = hit.GetComponentInParent<PlatformerPlayerHealth>();

                if (hitHealth == null || !hitHealth.IsAlive) continue;

                hitHealth.TakeDamage(damage);

                // Knockback: pushed away from the boss and upward.
                Vector2 away = (Vector2)hitHealth.transform.position - center;
                float sideX = Mathf.Abs(away.x) > 0.01f ? Mathf.Sign(away.x) : 1f;

                PlatformerPlayerMovement movement = hitHealth.GetComponent<PlatformerPlayerMovement>();
                if (movement != null)
                    movement.ApplyKnockback(new Vector2(sideX * slamKnockbackSide, slamKnockbackUp));
            }

            if (cameraFlow != null)
                cameraFlow.ShakeEffect(slamCameraShakeStrength, telegraphShakeVibrato, telegraphDuration * 2f);
        }

        private void CreateLandingMarker()
        {
            landingMarker = new GameObject("SlamLandingMarker");

            landingMarkerRenderer = landingMarker.AddComponent<SpriteRenderer>();
            landingMarkerRenderer.sprite = CreateFilledCircleSprite();
            landingMarkerRenderer.color = new Color(1f, 0.25f, 0.2f, 0.45f);
            landingMarkerRenderer.sortingOrder = 10;

            landingMarker.transform.localScale = Vector3.one * (slamRadius * 2f);
            landingMarker.SetActive(false);
        }

        private void SetLandingMarker(float x, float groundY, bool locked)
        {
            if (landingMarker == null) return;

            landingMarker.SetActive(true);
            landingMarker.transform.position = new Vector3(x, groundY + slamRadius, transform.position.z);

            if (landingMarkerRenderer != null)
                landingMarkerRenderer.color = locked
                    ? new Color(1f, 0.15f, 0.1f, 0.8f)
                    : new Color(1f, 0.25f, 0.2f, 0.45f);
        }

        private bool FindGroundAt(float x, float fromY, out float groundY)
        {
            groundY = fromY;

            Vector2 origin = new Vector2(x, fromY + 10f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, 100f, wallMask);

            float closest = float.PositiveInfinity;
            bool found = false;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == damageCollider) continue;
                if (hit.collider.GetComponentInParent<PlatformerPlayerHealth>() != null) continue;
                if (hit.distance >= closest) continue;

                closest = hit.distance;
                found = true;
            }

            if (found) groundY = origin.y - closest;

            return found;
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
                hitHealth.TakeDamage(damage, center);

                return;
            }
        }

        private static Sprite CreateFilledCircleSprite()
        {
            const int size = 64;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(radius - distance + 0.5f);

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void OnDrawGizmosSelected()
        {
            if (damageCollider == null) damageCollider = GetComponent<Collider2D>();

            Gizmos.color = Color.magenta;
            if (damageCollider != null)
                Gizmos.DrawWireCube(damageCollider.bounds.center, damageCollider.bounds.size);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, slamRadius);
        }
    }
}
