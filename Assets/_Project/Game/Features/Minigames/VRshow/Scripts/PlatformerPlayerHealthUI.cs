using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Minigames
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PlatformerPlayerHealthUI : MonoBehaviour
    {
        [Header("Binding")]
        [SerializeField] private PlatformerPlayerHealth health;
        [Tooltip("Second player (e.g. the FlyEvilMisty arena player) whose death also runs the scene restart. Its health is not shown as masks.")]
        [SerializeField] private PlatformerPlayerHealth secondaryHealth;

        [Header("Masks")]
        [SerializeField] private Sprite maskSprite;
        [SerializeField] private Sprite vesselSprite;
        [SerializeField, Min(8f)] private float maskSize = 48f;
        [SerializeField, Min(0f)] private float maskSpacing = 8f;

        [Header("Animations")]
        [SerializeField] private float breakDuration = 0.3f;
        [SerializeField] private float breakScale = 0.55f;
        [SerializeField] private float breakRotation = 35f;
        [SerializeField] private float healDuration = 0.35f;

        [Header("Death Fade")]
        [SerializeField] private Image blackScreen;
        [SerializeField, Min(0f)] private float deathFadeDuration = 0.5f;

        private static readonly Color MaskFill = new Color(0.92f, 0.89f, 0.78f);
        private static readonly Color MaskOutline = new Color(0.08f, 0.07f, 0.06f);
        private static readonly Color VesselFill = new Color(0.16f, 0.15f, 0.13f, 0.6f);
        private static readonly Color VesselOutline = new Color(0.3f, 0.28f, 0.25f, 0.8f);

        private readonly List<Image> fills = new List<Image>();

        private void Start()
        {
            if (health == null) health = FindFirstObjectByType<PlatformerPlayerHealth>();

            BuildMasks();

            if (blackScreen != null)
            {
                blackScreen.raycastTarget = false;
                blackScreen.SetAlpha(0f);
            }

            if (health == null) return;

            health.HealthChanged += OnMainHealthChanged;
            health.Died += OnDied;
            OnHealthChanged(health);

            if (secondaryHealth != null)
            {
                secondaryHealth.HealthChanged += OnSecondaryHealthChanged;
                secondaryHealth.Died += OnSecondaryDied;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.HealthChanged -= OnMainHealthChanged;
                health.Died -= OnDied;
            }

            if (secondaryHealth != null)
            {
                secondaryHealth.HealthChanged -= OnSecondaryHealthChanged;
                secondaryHealth.Died -= OnSecondaryDied;
            }
        }

        private void OnMainHealthChanged(int current, int max)
        {
            OnHealthChanged(health);
        }

        private void OnSecondaryHealthChanged(int current, int max)
        {
            OnHealthChanged(secondaryHealth);
        }

        private void OnHealthChanged(PlatformerPlayerHealth source)
        {
            // The two players share one mask row: only the active one drives it.
            if (source == null || !source.gameObject.activeInHierarchy) return;

            EnsureMaskCount(source.MaxHealth);

            for (int i = 0; i < fills.Count; i++)
            {
                if (i < source.CurrentHealth) ShowMask(fills[i]);
                else BreakMask(fills[i]);
            }
        }

        private void EnsureMaskCount(int count)
        {
            if (fills.Count == count) return;

            // Rebuild the row for a different max health.
            foreach (Image fill in fills)
            {
                if (fill != null) Destroy(fill.transform.parent.gameObject);
            }

            fills.Clear();

            for (int i = 0; i < count; i++)
            {
                RectTransform slot = CreateSlot(i);
                CreateVessel(slot);
                fills.Add(CreateFill(slot));
            }
        }

        private void OnDied()
        {
            StartCoroutine(DeathSequence(health));
        }

        private void OnSecondaryDied()
        {
            StartCoroutine(DeathSequence(secondaryHealth));
        }

        private IEnumerator DeathSequence(PlatformerPlayerHealth sourceHealth)
        {
            yield return new WaitForSeconds(sourceHealth.DeathShakeDuration);

            if (blackScreen != null)
            {
                blackScreen.raycastTarget = true;
                blackScreen.SetAlpha(0f);
                blackScreen.DOFade(1f, deathFadeDuration);
                yield return new WaitForSeconds(deathFadeDuration);
            }

            Canvas canvas = blackScreen != null ? blackScreen.GetComponentInParent<Canvas>(true) : null;
            if (canvas != null) DontDestroyOnLoad(canvas.gameObject);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield return null;

            if (blackScreen == null || canvas == null) yield break;

            blackScreen.DOFade(0f, deathFadeDuration)
                .OnComplete(() => Destroy(canvas.gameObject));
        }

        private void BuildMasks()
        {
            int count = health != null ? health.MaxHealth : 0;

            for (int i = 0; i < count; i++)
            {
                RectTransform slot = CreateSlot(i);
                CreateVessel(slot);
                fills.Add(CreateFill(slot));
            }
        }

        private RectTransform CreateSlot(int index)
        {
            RectTransform slot = new GameObject("Mask_" + index, typeof(RectTransform)).GetComponent<RectTransform>();
            slot.SetParent(transform, false);

            slot.anchorMin = new Vector2(0f, 1f);
            slot.anchorMax = new Vector2(0f, 1f);
            slot.pivot = new Vector2(0f, 1f);
            slot.anchoredPosition = new Vector2(index * (maskSize + maskSpacing), 0f);
            slot.sizeDelta = new Vector2(maskSize, maskSize);

            return slot;
        }

        private void CreateVessel(RectTransform slot)
        {
            if (vesselSprite == null) vesselSprite = CreateCircleSprite(VesselFill, VesselOutline);

            Image vessel = CreateGraphic("Vessel", slot, vesselSprite);
            vessel.color = Color.white;
        }

        private Image CreateFill(RectTransform slot)
        {
            if (maskSprite == null) maskSprite = CreateCircleSprite(MaskFill, MaskOutline);

            Image fill = CreateGraphic("Fill", slot, maskSprite);
            fill.color = Color.white;

            return fill;
        }

        private Image CreateGraphic(string name, RectTransform parent, Sprite sprite)
        {
            Image image = new GameObject(name, typeof(RectTransform)).AddComponent<Image>();

            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;

            RectTransform rect = image.rectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return image;
        }

        private void ShowMask(Image fill)
        {
            fill.DOKill();

            if (fill.gameObject.activeSelf) return;

            fill.gameObject.SetActive(true);
            fill.SetAlpha(0f);
            fill.rectTransform.localRotation = Quaternion.identity;
            fill.rectTransform.localScale = Vector3.one * 0.3f;

            fill.DOFade(1f, healDuration * 0.5f);
            fill.rectTransform.DOScale(Vector3.one, healDuration).SetEase(Ease.OutBack);
        }

        private void BreakMask(Image fill)
        {
            fill.DOKill();

            if (!fill.gameObject.activeSelf) return;

            fill.rectTransform.DOScale(Vector3.one * breakScale, breakDuration).SetEase(Ease.InBack);
            fill.rectTransform.DOLocalRotate(new Vector3(0f, 0f, breakRotation), breakDuration).SetEase(Ease.InBack);
            fill.DOFade(0f, breakDuration)
                .OnComplete(() =>
                {
                    fill.gameObject.SetActive(false);
                    fill.rectTransform.localRotation = Quaternion.identity;
                    fill.rectTransform.localScale = Vector3.one;
                });
        }

        private static Sprite CreateCircleSprite(Color fill, Color outline)
        {
            const int size = 64;
            const float ringWidth = 4f;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f - 1.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    Color color = distance < radius - ringWidth ? fill : outline;
                    color.a *= Mathf.Clamp01(radius - distance + 0.5f);

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }

    internal static class HealthUIExtensions
    {
        public static void SetAlpha(this Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
