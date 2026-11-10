using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public sealed class MonsterDash : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField, Min(0f)] private float detectionRadius = 6f;

        [Header("Dash")]
        [SerializeField, Min(0.01f)] private float dashSpeed = 18f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.2f;
        [SerializeField, Min(0f)] private float dashCooldown = 2f;

        [Header("Telegraph")]
        [SerializeField, Min(0f)] private float shakeDuration = 1f;
        [SerializeField, Min(0f)] private float shakeStrength = 0.15f;
        [SerializeField, Min(1)] private int shakeVibrato = 10;

        [Header("Braking")]
        [SerializeField, Min(0f)] private float brakeDuration = 0.25f;

        [Header("References")]
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private MonsterHealth monsterHealth;
        [SerializeField] private PlatformerPlayerHealth playerHealth;

        private float nextDashTime = -Mathf.Infinity;
        private Coroutine dashCoroutine;
        private bool isDashing;

        private Vector3 originalLocalPosition;
        private Tween shakeTween;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlatformerPlayerHealth>();

            originalLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            if (isDashing || Time.time < nextDashTime) return;
            if (monsterHealth != null && !monsterHealth.IsAlive) return;
            if (playerHealth == null || !playerHealth.IsAlive) return;

            Vector2 toPlayer = (Vector2)playerHealth.transform.position - (Vector2)transform.position;

            if (toPlayer.sqrMagnitude > detectionRadius * detectionRadius) return;
            if (toPlayer.sqrMagnitude < 0.0001f) return;

            nextDashTime = Time.time + dashCooldown;
            dashCoroutine = StartCoroutine(Dash(toPlayer.normalized));
        }

        private IEnumerator Dash(Vector2 direction)
        {
            if (body == null) yield break;

            isDashing = true;

            // --- Telegraph phase: shake in place before dashing ---
            if (shakeDuration > 0f)
            {
                // Kill any leftover tween and reset to base position first

                shakeTween = transform
                    .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, fadeOut: true);

                float telegraphElapsed = 0f;
                while (telegraphElapsed < shakeDuration)
                {
                    if (monsterHealth != null && !monsterHealth.IsAlive) break;
                    telegraphElapsed += Time.deltaTime;
                    yield return null;
                }

                shakeTween?.Kill();
                shakeTween = null;
            }

            // Bail out if the monster died during the telegraph
            if (monsterHealth != null && !monsterHealth.IsAlive)
            {
                isDashing = false;
                dashCoroutine = null;
                yield break;
            }

            // --- Dash phase ---
            float elapsed = 0f;

            while (elapsed < dashDuration)
            {
                if (monsterHealth != null && !monsterHealth.IsAlive) break;

                body.linearVelocity = direction * dashSpeed;
                elapsed += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }

            // --- Braking phase: smoothly decelerate from dashSpeed to 0 ---
            if (brakeDuration > 0f && body.linearVelocity.sqrMagnitude > 0.0001f)
            {
                float brakeElapsed = 0f;
                Vector2 startVelocity = body.linearVelocity;

                while (brakeElapsed < brakeDuration)
                {
                    if (monsterHealth != null && !monsterHealth.IsAlive) break;

                    brakeElapsed += Time.fixedDeltaTime;
                    float t = Mathf.Clamp01(brakeElapsed / brakeDuration);

                    float eased = 1f - (1f - t) * (1f - t);
                    body.linearVelocity = Vector2.Lerp(startVelocity, Vector2.zero, eased);

                    yield return new WaitForFixedUpdate();
                }
            }

            body.linearVelocity = Vector2.zero;

            isDashing = false;
            dashCoroutine = null;
        }

        private void OnDisable()
        {
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);

            shakeTween?.Kill();
            shakeTween = null;
            transform.localPosition = originalLocalPosition;

            dashCoroutine = null;
            isDashing = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}