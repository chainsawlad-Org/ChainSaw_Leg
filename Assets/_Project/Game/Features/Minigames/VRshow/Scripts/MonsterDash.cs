using System.Collections;
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

        [Header("References")]
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private MonsterHealth monsterHealth;
        [SerializeField] private PlatformerPlayerHealth playerHealth;

        private float nextDashTime = -Mathf.Infinity;
        private Coroutine dashCoroutine;
        private bool isDashing;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (monsterHealth == null) monsterHealth = GetComponent<MonsterHealth>();
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlatformerPlayerHealth>();
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

            float elapsed = 0f;

            while (elapsed < dashDuration)
            {
                if (monsterHealth != null && !monsterHealth.IsAlive) break;

                body.linearVelocity = direction * dashSpeed;
                elapsed += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }

            body.linearVelocity = Vector2.zero;

            isDashing = false;
            dashCoroutine = null;
        }

        private void OnDisable()
        {
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);

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
