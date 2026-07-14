using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace ChainSawLeg.Features.Exploration
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private GameObject collidersObject;
        [SerializeField] private SpriteRenderer[] roomSprites;

        private float transitionDuration = 1f;

        public void OpenRoom()
        {
            collidersObject.SetActive(true);
            foreach (var sprite in roomSprites)
            {
                DOVirtual.Color(new Color(0.2f, 0.2f, 0.2f, 1f), Color.white, transitionDuration * 0.5f, (Color color) => sprite.color = color);
                sprite.sortingOrder++;
            }
        }

        public void CloseRoom(Action onClose)
        {
            
            collidersObject.SetActive(false);
            foreach (var sprite in roomSprites)
            {
                DOVirtual.Color(Color.white, new Color(0.2f, 0.2f, 0.2f, 1f), transitionDuration, (Color color) => sprite.color = color);
                DOVirtual.DelayedCall(transitionDuration * 0.5f, () => sprite.sortingOrder--);
            }
            DOVirtual.DelayedCall(transitionDuration * 0.5f, () => onClose?.Invoke());
        }
    }
}
