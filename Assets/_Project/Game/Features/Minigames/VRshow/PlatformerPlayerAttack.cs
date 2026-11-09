using System.Collections;
using System.Collections.Generic;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class PlatformerPlayerAttack : MonoBehaviour
    {
        [SerializeField] private Sprite attackSprite;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private PlatformerPlayerMovement movement;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private float attackDuration = 0.5f;
        [SerializeField] private float shakeStrength = 0.2f;
        [SerializeField] private int shakeVibrato = 100;
        [SerializeField] private float shakeDuration = 0.4f;

        [Header("Hit Detection")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float hitScanDuration = 0.15f;
        [SerializeField] private float attackRadius = 1.1f;
        [SerializeField] private Vector2 attackOffset = new Vector2(1f, 0f);
        [SerializeField] private LayerMask damageableMask = ~0;
        [SerializeField] private Collider2D bodyCollider;

        [Header("Airborne Area Attack")]
        [SerializeField] private float areaAttackRadius = 2.2f;
        [SerializeField] private int areaAttackDamage = 2;
        [SerializeField] private float areaHitScanDuration = 0.2f;
        [SerializeField] private float upwardLaunchForce = 18f;

        private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        private Sprite defaultSprite;
        private bool isAttacking;
        private bool hasLaunched;
        private Coroutine hitScanCoroutine;

        private void Awake()
        {
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();

            defaultSprite = sprite.sprite;
        }

        private void Update()
        {
            if (isAttacking || !Keyboard.current.spaceKey.wasPressedThisFrame) return;

            if (Keyboard.current.sKey.isPressed && !movement.IsGrounded())
                AreaAttack();
            else
                Attack();
        }

        private void Attack()
        {
            BeginAttack(false, attackRadius, damage, hitScanDuration);
        }

        private void AreaAttack()
        {
            BeginAttack(true, areaAttackRadius, areaAttackDamage, areaHitScanDuration);
        }

        private void BeginAttack(bool isAreaAttack, float radius, int attackDamage, float scanDuration)
        {
            isAttacking = true;
            hasLaunched = false;
            damagedTargets.Clear();
            sprite.sprite = attackSprite;
            cameraFlow.ShakeEffect(shakeStrength, shakeVibrato, shakeDuration);

            if (hitScanCoroutine != null) StopCoroutine(hitScanCoroutine);
            hitScanCoroutine = StartCoroutine(ScanForHits(isAreaAttack, radius, attackDamage, scanDuration));

            DOVirtual.DelayedCall(attackDuration, EndAttack);
        }

        private IEnumerator ScanForHits(bool isAreaAttack, float radius, int attackDamage, float scanDuration)
        {
            float scanEndTime = Time.time + scanDuration;

            do
            {
                PerformHitDetection(isAreaAttack, radius, attackDamage);
                yield return null;
            }
            while (isAttacking && Time.time < scanEndTime);

            hitScanCoroutine = null;
        }

        private void PerformHitDetection(bool isAreaAttack, float radius, int attackDamage)
        {
            Vector2 attackCenter = isAreaAttack ? GetAreaCenter() : GetAttackCenter();
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, radius, damageableMask);

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();

                if (damageable == null || !damagedTargets.Add(damageable)) continue;

                damageable.TakeDamage(attackDamage);

                if (isAreaAttack)
                    LaunchUp();
            }
        }

        private void LaunchUp()
        {
            if (hasLaunched) return;

            hasLaunched = true;
            movement.LaunchUp(upwardLaunchForce);
        }

        private Vector2 GetAttackCenter()
        {
            Vector2 origin = bodyCollider != null
                ? (Vector2)bodyCollider.bounds.center
                : (Vector2)transform.position;

            float facing = sprite.flipX ? -1f : 1f;

            return origin + new Vector2(attackOffset.x * facing, attackOffset.y);
        }

        private Vector2 GetAreaCenter()
        {
            return bodyCollider != null
                ? (Vector2)bodyCollider.bounds.center
                : (Vector2)transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            if (sprite == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetAttackCenter(), attackRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetAreaCenter(), areaAttackRadius);
        }

        private void EndAttack()
        {
            isAttacking = false;
            sprite.sprite = defaultSprite;
        }
    }
}
