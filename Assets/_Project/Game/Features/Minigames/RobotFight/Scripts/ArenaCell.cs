using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Visual state of a single arena tile: spawn/despawn fades and recolouring between
    /// default, danger and beatable states. Purely presentational.
    /// </summary>
    public class ArenaCell : MonoBehaviour
    {
        private const float HiddenAlpha = 0f;
        private const float VisibleAlpha = 1f;

        [SerializeField] private float recolorTime = 0.5f;
        [SerializeField] private SpriteRenderer[] lineSprites;
        [SerializeField] private SpriteRenderer stripeSprite;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color dangerColor;
        [SerializeField] private Color beatableColor;
        [SerializeField] private Material dangerMaterial;
        [SerializeField] private Material beatableMaterial;

        /// <summary>True while the cell is in its neutral (standable / attack-target) state.</summary>
        public bool inDefaultState { get; private set; } = true;

        private void Start()
        {
            foreach (SpriteRenderer line in lineSprites)
            {
                line.color = WithAlpha(line.color, HiddenAlpha);
            }
        }

        /// <summary>Fades the tile lines in from transparent.</summary>
        public void DoSpawnAnimation()
        {
            Color from = WithAlpha(lineSprites[0].color, HiddenAlpha);
            foreach (SpriteRenderer sprite in lineSprites)
            {
                DOVirtual.Color(from, defaultColor, recolorTime, x => sprite.color = x);
            }
        }

        /// <summary>Fades the tile lines out, then destroys the tile.</summary>
        public void Despawn()
        {
            Color target = WithAlpha(lineSprites[0].color, HiddenAlpha);
            foreach (SpriteRenderer sprite in lineSprites)
            {
                DOVirtual.Color(sprite.color, target, recolorTime, x => sprite.color = x)
                    .OnComplete(() => Destroy(gameObject));
            }
        }

        /// <summary>Reverts the tile to its neutral state.</summary>
        public void SetDefaultCell()
        {
            TransitionLines(defaultColor);
            TransitionStripe(WithAlpha(stripeSprite.color, HiddenAlpha));
            inDefaultState = true;
        }

        /// <summary>Highlights the tile as an incoming attack's danger zone.</summary>
        public void SetDangerCell() => SetActiveState(dangerColor, dangerMaterial);

        /// <summary>Highlights the tile as holding a block the player can kick back.</summary>
        public void SetBeatableCell() => SetActiveState(beatableColor, beatableMaterial);

        private void SetActiveState(Color lineColor, Material stripeMaterial)
        {
            TransitionLines(lineColor);
            TransitionStripe(WithAlpha(stripeSprite.color, VisibleAlpha));
            stripeSprite.material = stripeMaterial;
            inDefaultState = false;
        }

        private void TransitionLines(Color target)
        {
            Color from = lineSprites[0].color;
            foreach (SpriteRenderer sprite in lineSprites)
            {
                DOVirtual.Color(from, target, recolorTime, x => sprite.color = x);
            }
        }

        private void TransitionStripe(Color target)
        {
            DOVirtual.Color(stripeSprite.color, target, recolorTime, x => stripeSprite.color = x);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
