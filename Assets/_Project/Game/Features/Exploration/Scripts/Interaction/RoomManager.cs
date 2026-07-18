using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Zenject;
using ChainSawLeg.Features.Exploration.Save;

namespace ChainSawLeg.Features.Exploration
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private GameObject collidersObject;
        [SerializeField] private SpriteRenderer[] roomSprites;
        [SerializeField] private Camera targetCamera;
        private GameplayInputBlockService gameplayInputBlockService;

        private float transitionDuration = 1f;

        [Inject]
        public void Construct(GameplayInputBlockService gameplayInputBlockService)
        {
            this.gameplayInputBlockService = gameplayInputBlockService;
        }
        
        public void OpenRoom()
        {
            collidersObject.SetActive(true);
            foreach (var sprite in roomSprites)
            {
                DOVirtual.Color(new Color(0.2f, 0.2f, 0.2f, 1f), Color.white, transitionDuration * 0.5f, (Color color) => sprite.color = color);
                sprite.sortingOrder++;
            }
            DOVirtual.DelayedCall(transitionDuration * 0.5f, () => gameplayInputBlockService.ReleaseBlock(InputBlockChannels.Gameplay));
        }

        public void CloseRoom(Vector2 nextRoomPosition, Action onClose)
        {
            Vector3 target = new Vector3(nextRoomPosition.x, nextRoomPosition.y, targetCamera.transform.position.z);
            targetCamera.transform.DOMove(target, transitionDuration).SetEase(Ease.InOutQuart);

            gameplayInputBlockService.AcquireBlock(InputBlockChannels.Gameplay);
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
