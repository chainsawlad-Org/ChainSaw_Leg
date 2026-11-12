using System;
using System.Collections;
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

        [Header("Hit Knockback")]
        [SerializeField, Min(0f)] private float knockbackSide = 8f;
        [SerializeField, Min(0f)] private float knockbackUp = 6f;

        [Header("Hit Time Scale")]
        [SerializeField, Range(0.01f, 1f)] private float hitTimeScale = 0.2f;
        [SerializeField, Min(0f)] private float hitTimeScaleFadeIn = 0.05f;
        [SerializeField, Min(0f)] private float hitTimeScaleHold = 0.1f;
        [SerializeField, Min(0f)] private float hitTimeScaleFadeOut = 0.2f;

        [Header("Hurt Sprite")]
        [SerializeField] private Sprite hurtSprite;
        [SerializeField, Min(0f)] private float hurtSpriteDuration = 1f;

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
        private Sprite defaultSprite;
        private bool isDead;
        private float invulnerableUntil = -Mathf.Infinity;
        private Sequence hitTimeScaleSequence;
        private Coroutine hurtSpriteCoroutine;

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

            if (sprite != null)
            {
                defaultColor = sprite.color;
                defaultSprite = sprite.sprite;
            }

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
            TakeDamageCore(amount, null);
        }

        public void TakeDamage(int amount, Vector2 sourcePosition)
        {
            TakeDamageCore(amount, sourcePosition);
        }

        private void TakeDamageCore(int amount, Vector2? sourcePosition)
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

            PlayHurtSprite();
            PlayHitTimeScale();

            if (sourcePosition.HasValue) ApplyHitKnockback(sourcePosition.Value);
        }

        private void PlayHurtSprite()
        {
            if (sprite == null || hurtSprite == null) return;

            sprite.sprite = hurtSprite;

            if (hurtSpriteCoroutine != null) StopCoroutine(hurtSpriteCoroutine);
            hurtSpriteCoroutine = StartCoroutine(HurtSpriteRoutine());
        }

        private IEnumerator HurtSpriteRoutine()
        {
            yield return new WaitForSeconds(hurtSpriteDuration);

            hurtSpriteCoroutine = null;

            // Only revert if nothing else swapped the sprite in the meantime.
            if (sprite != null && sprite.sprite == hurtSprite) sprite.sprite = defaultSprite;
        }

        private void PlayHitTimeScale()
        {
            const float defaultTimeScale = 1f;

            if (hitTimeScaleSequence != null && hitTimeScaleSequence.IsActive())
                hitTimeScaleSequence.Kill();

            // Runs on unscaled time so the effect itself is not slowed by it.
            hitTimeScaleSequence = DOTween.Sequence().SetUpdate(true);
            hitTimeScaleSequence.Append(DOTween.To(() => Time.timeScale, value => Time.timeScale = value, hitTimeScale, hitTimeScaleFadeIn).SetUpdate(true));
            hitTimeScaleSequence.AppendInterval(hitTimeScaleHold);
            hitTimeScaleSequence.Append(DOTween.To(() => Time.timeScale, value => Time.timeScale = value, defaultTimeScale, hitTimeScaleFadeOut).SetUpdate(true));
            hitTimeScaleSequence.OnKill(() => Time.timeScale = defaultTimeScale);
        }

        private void OnDestroy()
        {
            if (hitTimeScaleSequence != null && hitTimeScaleSequence.IsActive())
                hitTimeScaleSequence.Kill();

            if (hurtSpriteCoroutine != null) StopCoroutine(hurtSpriteCoroutine);

            Time.timeScale = 1f;
        }

        private void ApplyHitKnockback(Vector2 sourcePosition)
        {
            if (movement == null) return;

            // Push away from the attacker, slightly upward.
            Vector2 away = (Vector2)transform.position - sourcePosition;
            float sideX = Mathf.Abs(away.x) > 0.01f ? Mathf.Sign(away.x) : 1f;

            movement.ApplyKnockback(new Vector2(sideX * knockbackSide, knockbackUp));
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0 || CurrentHealth >= maxHealth) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Healed?.Invoke(CurrentHealth, maxHealth);
        }

        public void GrantInvulnerability(float duration)
        {
            if (!IsAlive || duration <= 0f) return;

            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + duration);
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
            AudioService.Play(AudioId.DamageTaken);
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
