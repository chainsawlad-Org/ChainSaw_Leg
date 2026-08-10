using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using Zenject;

namespace ChainSawLeg.Features.Exploration
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private GameObject collidersObject;
        [SerializeField] private SpriteRenderer[] roomSprites;
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private Transform leftUpperBound;
        [SerializeField] private Transform rightLowerBound;
        [SerializeField] private bool isStartRoom;
        private GameplayInputBlockService gameplayInputBlockService;
        [HideInInspector] public Bounds roomBounds;
        private CameraFlow cameraFlow;

        private float transitionDuration = 1f;

        [Inject]
        public void Construct(GameplayInputBlockService gameplayInputBlockService, CameraFlow cameraFlow)
        {
            this.gameplayInputBlockService = gameplayInputBlockService;
            roomBounds = CreateBoundsFromTransforms(leftUpperBound.position, rightLowerBound.position);
            this.cameraFlow = cameraFlow;
        }

        private void Start()
        {
            if (isStartRoom)
            {
                cameraFlow.bounds = roomBounds;
                sortingGroup.enabled = false;
            }
            else
            {
                foreach (var sprite in roomSprites) sprite.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                sortingGroup.sortingOrder--;
                collidersObject.SetActive(false);
            }
        }

        public void OpenRoom()
        {
            collidersObject.SetActive(true);
            foreach (var sprite in roomSprites)
            {
                DOVirtual.Color(new Color(0.2f, 0.2f, 0.2f, 1f), Color.white, transitionDuration * 0.5f, (Color color) => sprite.color = color);
                sortingGroup.sortingOrder++;
            }
            DOVirtual.DelayedCall(transitionDuration * 0.5f, () => gameplayInputBlockService.ReleaseBlock(InputBlockChannels.Gameplay));
            cameraFlow.bounds = roomBounds;
            sortingGroup.enabled = false;
        }

        public void CloseRoom(RoomManager nextRoom, Action onClose)
        {
            //Vector3 target = new Vector3(nextRoomPosition.x, nextRoomPosition.y, targetCamera.transform.position.z);
            //targetCamera.transform.DOMove(target, transitionDuration).SetEase(Ease.InOutQuart);
            cameraFlow.TransitToRoom(nextRoom.roomBounds, transitionDuration);

            gameplayInputBlockService.AcquireBlock(InputBlockChannels.Gameplay);
            collidersObject.SetActive(false);
            
            foreach (var sprite in roomSprites)
            {
                DOVirtual.Color(Color.white, new Color(0.2f, 0.2f, 0.2f, 1f), transitionDuration, (Color color) => sprite.color = color);
                DOVirtual.DelayedCall(transitionDuration * 0.5f, () => { sortingGroup.sortingOrder--; });
            }
            DOVirtual.DelayedCall(transitionDuration * 0.5f, () =>
            {
                sortingGroup.enabled = true;
                onClose?.Invoke();
            });
        }
        
        private Bounds CreateBoundsFromTransforms(Vector3 leftUpperBound, Vector3 rightLowerBound)
        {
            Vector3 center = (leftUpperBound + rightLowerBound) * 0.5f;
    
            Vector3 size = new Vector3(
                Mathf.Abs(rightLowerBound.x - leftUpperBound.x),
                Mathf.Abs(leftUpperBound.y - rightLowerBound.y),
                Mathf.Abs(rightLowerBound.z - leftUpperBound.z)
            );
    
            return new Bounds(center, size);
        }
    }
}
