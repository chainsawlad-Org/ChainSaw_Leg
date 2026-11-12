using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Plain-C# presenter for the arena HUD: healthbar slide in/out and fill animation,
    /// lose-screen fade and death shake. Driven by <see cref="ArenaBoard"/>.
    /// </summary>
    public class ArenaHudView
    {
        private readonly RectTransform healthbarTransform;
        private readonly Image healthbarImage;
        private readonly CanvasGroup loseScreen;
        private readonly float shownX;
        private readonly float slideDuration;
        private readonly float fillDuration;
        private readonly float loseFadeDuration;
        private readonly float loseShakeDuration;
        private readonly float loseShakeStrength;
        private readonly int loseShakeVibrato;
        private readonly float loseShakeRandomness;

        /// <summary>Creates the HUD presenter. All timing parameters default to the original hardcoded values.</summary>
        public ArenaHudView(RectTransform healthbarTransform, Image healthbarImage, CanvasGroup loseScreen,
            float shownX, float slideDuration, float fillDuration,
            float loseFadeDuration, float loseShakeDuration, float loseShakeStrength,
            int loseShakeVibrato, float loseShakeRandomness)
        {
            this.healthbarTransform = healthbarTransform;
            this.healthbarImage = healthbarImage;
            this.loseScreen = loseScreen;
            this.shownX = shownX;
            this.slideDuration = slideDuration;
            this.fillDuration = fillDuration;
            this.loseFadeDuration = loseFadeDuration;
            this.loseShakeDuration = loseShakeDuration;
            this.loseShakeStrength = loseShakeStrength;
            this.loseShakeVibrato = loseShakeVibrato;
            this.loseShakeRandomness = loseShakeRandomness;
        }

        /// <summary>Slides the healthbar on screen.</summary>
        public void ShowHealthbar() => healthbarTransform.DOMoveX(shownX, slideDuration);

        /// <summary>Slides the healthbar off screen.</summary>
        public void HideHealthbar() => healthbarTransform.DOMoveX(-shownX, slideDuration);

        /// <summary>Tweens the healthbar fill to the given raw health fraction.</summary>
        public void SetHealthFraction(float fraction) =>
            DOVirtual.Float(healthbarImage.fillAmount, fraction, fillDuration, x => healthbarImage.fillAmount = x);

        /// <summary>Fades the lose screen in and shakes the healthbar (player death presentation).</summary>
        public void ShowLoseScreen()
        {
            DOVirtual.Float(0f, 1f, loseFadeDuration, x => loseScreen.alpha = x);
            healthbarTransform.DOShakePosition(loseShakeDuration, loseShakeStrength, loseShakeVibrato, loseShakeRandomness, false, false);
        }
    }
}
