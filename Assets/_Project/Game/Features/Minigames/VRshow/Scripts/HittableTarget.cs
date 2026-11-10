using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public sealed class HittableTarget : MonoBehaviour, IDamageable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private float hitFlashDuration = 0.1f;

        private Color defaultColor;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) defaultColor = spriteRenderer.color;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || spriteRenderer == null) return;

            spriteRenderer.DOKill();
            spriteRenderer.color = hitColor;
            spriteRenderer.DOColor(defaultColor, hitFlashDuration);
        }
    }
}
