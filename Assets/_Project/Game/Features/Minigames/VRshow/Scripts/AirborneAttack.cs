using System.Collections;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Airborne attack for the FlyEvilMisty boss fight. On Enter (hardcoded, same
    /// style as PlatformerPlayerMovement/PlatformerPlayerAttack) the player strikes
    /// in place: the view swaps to the spinning <see cref="airborneSkin"/> child for
    /// the attack duration, same pattern as the platformer's AirAttackSprite. Any
    /// IDamageable inside <see cref="attackRange"/> takes <see cref="attackDamage"/>
    /// damage — this is how the boss is hurt (1 hit per attack) and it also clears
    /// triangle missiles caught in the strike. A hit that lands on a MonsterHealth
    /// (the boss) shakes the camera and knocks the player back away from the target
    /// so they have to fly in again. While attacking, MonsterAttack on the boss is
    /// gated off via <see cref="IsAttacking"/> so touching the boss mid-strike is safe.
    /// </summary>
    public sealed class AirborneAttack : MonoBehaviour
    {
        [Header("Attack")]
        [Tooltip("Damageable targets within this distance of the player take damage.")]
        [SerializeField, Min(0.1f)] private float attackRange = 3f;
        [SerializeField, Min(1)] private int attackDamage = 1;
        [SerializeField, Min(0f)] private float attackCooldown = 0.5f;
        [Tooltip("How long the attack state (spinning skin) lasts.")]
        [SerializeField, Min(0f)] private float attackDuration = 0.5f;
        [SerializeField] private LayerMask damageableMask = ~0;
        [Tooltip("How long the strike keeps looking for targets after it starts.")]
        [SerializeField, Min(0f)] private float hitScanDuration = 0.25f;

        [Header("Airborne Skin")]
        [Tooltip("Child object shown instead of the player sprite while attacking. Falls back to a child named 'AirAttackSprite'.")]
        [SerializeField] private Transform airborneSkin;
        [SerializeField, Min(0f)] private float airborneSkinSpinSpeed = 360f;

        [Header("Hit Feedback")]
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private float shakeStrength = 0.2f;
        [SerializeField] private int shakeVibrato = 100;
        [SerializeField] private float shakeDuration = 0.4f;

        [Header("Hit Knockback")]
        [SerializeField, Min(0f)] private float knockbackForce = 28f;
        [SerializeField, Min(0f)] private float knockbackControlTime = 0.5f;

        [Header("References")]
        [Tooltip("The boss the strike targets. Falls back to the first FlyEvilMistyBoss in the scene.")]
        [SerializeField] private Transform attackTarget;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private FlyEvilMistyPlayerController controller;
        [Tooltip("Optional effect spawned where the attack lands.")]
        [SerializeField] private GameObject attackEffectPrefab;

        private float nextAttackTime = -Mathf.Infinity;
        private bool hasShaken;
        private Coroutine hitScanCoroutine;

        /// <summary>True while the attack state is active; MonsterAttack gates on this.</summary>
        public bool IsAttacking { get; private set; }

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (controller == null) controller = GetComponent<FlyEvilMistyPlayerController>();
            if (sprite == null) sprite = GetComponent<SpriteRenderer>();
            if (airborneSkin == null) airborneSkin = transform.Find("AirAttackSprite");
            if (cameraFlow == null) cameraFlow = FindFirstObjectByType<CameraFlow>();
        }

        private void OnEnable()
        {
            ResolveAttackTarget();
        }

        private void OnDisable()
        {
            EndAttack();

            if (hitScanCoroutine != null)
            {
                StopCoroutine(hitScanCoroutine);
                hitScanCoroutine = null;
            }
        }

        private void ResolveAttackTarget()
        {
            if (attackTarget != null) return;

            FlyEvilMistyBoss boss = FindFirstObjectByType<FlyEvilMistyBoss>();
            if (boss != null) attackTarget = boss.transform;
            else Debug.LogWarning("AirborneAttack: no attack target assigned and no FlyEvilMistyBoss found.", this);
        }

        private void Update()
        {
            if (IsAttacking && airborneSkin != null)
                airborneSkin.Rotate(0f, 0f, airborneSkinSpinSpeed * Time.deltaTime);

            if (IsAttacking || Keyboard.current == null || !Keyboard.current.enterKey.wasPressedThisFrame) return;
            if (Time.time < nextAttackTime) return;

            BeginAttack();
        }

        private void BeginAttack()
        {
            IsAttacking = true;
            nextAttackTime = Time.time + attackCooldown;
            hasShaken = false;

            // Airborne attack view: swap the whole view to the spinning skin,
            // same pattern as the platformer player's AirAttackSprite.
            if (sprite != null) sprite.enabled = false;
            if (airborneSkin != null) airborneSkin.gameObject.SetActive(true);

            AudioService.Play(AudioId.Swing);

            if (hitScanCoroutine != null) StopCoroutine(hitScanCoroutine);
            hitScanCoroutine = StartCoroutine(ScanForHits(hitScanDuration));

            DOVirtual.DelayedCall(attackDuration, EndAttack);
        }

        private void EndAttack()
        {
            IsAttacking = false;

            if (airborneSkin != null) airborneSkin.gameObject.SetActive(false);
            if (sprite != null) sprite.enabled = true;
        }

        private IEnumerator ScanForHits(float scanDuration)
        {
            float scanEndTime = Time.time + scanDuration;

            do
            {
                if (PerformHitDetection())
                {
                    hitScanCoroutine = null;
                    yield break;
                }

                yield return null;
            }
            while (Time.time < scanEndTime);

            hitScanCoroutine = null;
        }

        private bool PerformHitDetection()
        {
            Vector2 center = body.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRange, damageableMask);

            foreach (Collider2D hit in hits)
            {
                // Skip the player itself.
                if (hit.attachedRigidbody == body || hit.transform.IsChildOf(transform)) continue;

                // Triangles must stay a threat even mid-strike: they are damageable,
                // but the strike must not eat them before they reach the player.
                if (hit.GetComponentInParent<BossTriangleMissile>() != null) continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null) continue;

                damageable.TakeDamage(attackDamage);
                AudioService.Play(AudioId.AttackHit);

                if (attackEffectPrefab != null)
                    Instantiate(attackEffectPrefab, hit.transform.position, Quaternion.identity);

                // Hitting the boss (MonsterHealth) shakes the camera and knocks the
                // player back away from it. Triangle missiles are also damageable
                // but only get destroyed.
                if (hit.GetComponentInParent<MonsterHealth>() != null)
                {
                    ShakeOnHit();

                    Vector2 awayFromTarget = body.position - (Vector2)hit.transform.position;
                    Vector2 knockbackDirection = awayFromTarget.sqrMagnitude > 0.0001f
                        ? awayFromTarget.normalized
                        : Vector2.down;

                    body.linearVelocity = knockbackDirection * knockbackForce;

                    // The controller honours the suspension instead of fighting the knockback.
                    if (controller != null) controller.SuspendControl(knockbackControlTime);
                }

                return true;
            }

            return false;
        }

        private void ShakeOnHit()
        {
            if (hasShaken) return;

            hasShaken = true;

            // ensurePlay: a shake from a triangle hit at the same moment must not
            // swallow the boss-hit feedback.
            if (cameraFlow != null) cameraFlow.ShakeEffect(shakeStrength, shakeVibrato, shakeDuration, ensurePlay: true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector2 center = body != null ? body.position : (Vector2)transform.position;
            Gizmos.DrawWireSphere(center, attackRange);
        }
    }
}
