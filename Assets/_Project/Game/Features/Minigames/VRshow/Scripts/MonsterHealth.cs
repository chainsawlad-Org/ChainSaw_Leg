using System;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public sealed class MonsterHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 3;

        [Header("Death")]
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float deathDuration = 0.25f;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Rigidbody2D body;

        [Header("Hit Feedback")]
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float hitFlashDuration = 0.1f;

        private Color defaultColor;
        private bool isDead;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => !isDead && CurrentHealth > 0;

        public event Action<int, int> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
            if (body == null) body = GetComponent<Rigidbody2D>();

            if (spriteRenderer != null) defaultColor = spriteRenderer.color;

            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
                Die();
            else
                PlayHitFlash();
        }

        private void PlayHitFlash()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.DOKill();
            spriteRenderer.color = hitColor;
            spriteRenderer.DOColor(defaultColor, hitFlashDuration);
        }

        private void Die()
        {
            isDead = true;

            if (bodyCollider != null) bodyCollider.enabled = false;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            Died?.Invoke();

            if (!destroyOnDeath) return;

            if (spriteRenderer != null) spriteRenderer.DOKill();

            transform.DOScale(Vector3.zero, deathDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
