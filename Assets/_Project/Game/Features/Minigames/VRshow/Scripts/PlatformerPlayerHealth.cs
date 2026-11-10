using System;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public sealed class PlatformerPlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 5;

        [Header("Invulnerability")]
        [SerializeField] private float invulnerabilityDuration = 1f;
        [SerializeField] private float blinkInterval = 0.08f;

        [Header("Hit Feedback")]
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float hitFlashDuration = 0.1f;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private float shakeStrength = 0.3f;
        [SerializeField] private int shakeVibrato = 100;
        [SerializeField] private float shakeDuration = 0.4f;

        [Header("Death")]
        [SerializeField, Min(0f)] private float deathShakeDuration = 3f;
        [SerializeField] private float deathShakeStrength = 0.2f;
        [SerializeField] private int deathShakeVibrato = 60;

        [Header("References")]
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private PlatformerPlayerMovement movement;

        private Color defaultColor;
        private bool isDead;
        private float invulnerableUntil = -Mathf.Infinity;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => !isDead && CurrentHealth > 0;
        public bool IsInvulnerable => Time.time < invulnerableUntil;
        public float DeathShakeDuration => deathShakeDuration;

        public event Action<int, int> HealthChanged;
        public event Action<int, int> Damaged;
        public event Action<int, int> Healed;
        public event Action Died;

        private void Awake()
        {
            if (sprite == null) sprite = GetComponent<SpriteRenderer>();
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (movement == null) movement = GetComponent<PlatformerPlayerMovement>();

            if (sprite != null) defaultColor = sprite.color;

            CurrentHealth = maxHealth;

            if (PlatformerCheckpointData.HasPosition)
            {
                Vector3 position = PlatformerCheckpointData.Position;
                transform.position = position;

                if (body != null) body.position = position;
            }
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0 || IsInvulnerable) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Damaged?.Invoke(CurrentHealth, maxHealth);
            
            PlayHitFeedback();

            if (CurrentHealth == 0)
            {
                Die();
                return;
            }

            invulnerableUntil = Time.time + invulnerabilityDuration;
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0 || CurrentHealth >= maxHealth) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Healed?.Invoke(CurrentHealth, maxHealth);
        }

        public void Kill()
        {
            if (!IsAlive) return;

            CurrentHealth = 0;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Damaged?.Invoke(CurrentHealth, maxHealth);

            PlayHitFeedback();
            Die();
        }

        private void PlayHitFeedback()
        {
            if (cameraFlow != null) cameraFlow.ShakeEffect(shakeStrength, shakeVibrato, shakeDuration);
            if (sprite == null) return;

            float blinkTime = Mathf.Max(0f, invulnerabilityDuration - hitFlashDuration);
            int blinkLoops = Mathf.Max(1, Mathf.RoundToInt(blinkTime / (blinkInterval * 2f)));

            sprite.DOKill();
            sprite.color = hitColor;
            sprite.DOColor(defaultColor, hitFlashDuration);
            // sprite.DOColor(defaultColor, hitFlashDuration)
            //     .OnComplete(() =>
            //         sprite.DOFade(0.4f, blinkInterval)
            //             .SetLoops(blinkLoops, LoopType.Yoyo)
            //             .OnComplete(() => sprite.color = defaultColor));
        }

        private void Die()
        {
            isDead = true;
            
            if (movement != null) movement.LockMovement();

            if (bodyCollider != null) bodyCollider.enabled = false;

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            Died?.Invoke();

            transform.DOShakePosition(deathShakeDuration, deathShakeStrength, deathShakeVibrato, 90F, false, false);
        }
    }
}
