using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class ArenaCell : MonoBehaviour
    {
        [SerializeField] private float recolorTime = 0.5f;
        [SerializeField] private SpriteRenderer[] lineSprites;
        [SerializeField] private SpriteRenderer stripeSprite;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color dangerColor;
        [SerializeField] private Color beatableColor;
        [SerializeField] private Material dangerMaterial;
        [SerializeField] private Material beatableMaterial;
        
        public bool inDefaultState {get; private set;} = true;

        public void SetDefaultCell()
        {
            Color transparent = stripeSprite.color;
            transparent.a = 0;
            
            foreach (var sprite in lineSprites)
                DOVirtual.Color(lineSprites[0].color, defaultColor, recolorTime, onVirtualUpdate: x => sprite.color = x);
            DOVirtual.Color(stripeSprite.color, transparent, recolorTime, onVirtualUpdate: x => stripeSprite.color = x);

            inDefaultState = true;
        }
        
        public void SetDangerCell()
        {
            Color nonTransparent = stripeSprite.color;
            nonTransparent.a = 1f;
            
            foreach (var sprite in lineSprites)
                DOVirtual.Color(lineSprites[0].color, dangerColor, recolorTime, onVirtualUpdate: x => sprite.color = x);
            DOVirtual.Color(stripeSprite.color, nonTransparent, recolorTime, onVirtualUpdate: x => stripeSprite.color = x);
            stripeSprite.material = dangerMaterial;

            inDefaultState = false;
        }

        public void SetBeatableCell()
        {
            Color nonTransparent = stripeSprite.color;
            nonTransparent.a = 1f;
            
            foreach (var sprite in lineSprites)
                DOVirtual.Color(lineSprites[0].color, beatableColor, recolorTime, onVirtualUpdate: x => sprite.color = x);
            DOVirtual.Color(stripeSprite.color, nonTransparent, recolorTime, onVirtualUpdate: x => stripeSprite.color = x);
            stripeSprite.material = beatableMaterial;
            
            inDefaultState = false;
        }
    }
}
