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
        [SerializeField] private float attackRadius = 1.1f;
        [SerializeField] private Vector2 attackOffset = new Vector2(1f, 0f);
        [SerializeField] private LayerMask damageableMask = ~0;
        [SerializeField] private Collider2D bodyCollider;

        private Sprite defaultSprite;
        private bool isAttacking;

        private void Awake()
        {
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();

            defaultSprite = sprite.sprite;
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && !isAttacking)
                Attack();
        }

        private void Attack()
        {
            isAttacking = true;
            sprite.sprite = attackSprite;
            movement.LockMovement();
            cameraFlow.ShakeEffect(shakeStrength, shakeVibrato, shakeDuration);

            PerformHitDetection();

            DOVirtual.DelayedCall(attackDuration, EndAttack);
        }

        private void PerformHitDetection()
        {
            Vector2 attackCenter = GetAttackCenter();
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius, damageableMask);
            var damagedTargets = new HashSet<IDamageable>();

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();

                if (damageable == null || !damagedTargets.Add(damageable)) continue;

                damageable.TakeDamage(damage);
            }
        }

        private Vector2 GetAttackCenter()
        {
            Vector2 origin = bodyCollider != null
                ? (Vector2)bodyCollider.bounds.center
                : (Vector2)transform.position;

            float facing = sprite.flipX ? -1f : 1f;

            return origin + new Vector2(attackOffset.x * facing, attackOffset.y);
        }

        private void OnDrawGizmosSelected()
        {
            if (sprite == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetAttackCenter(), attackRadius);
        }

        private void EndAttack()
        {
            isAttacking = false;
            sprite.sprite = defaultSprite;
            movement.UnlockMovement();
        }
    }
}
