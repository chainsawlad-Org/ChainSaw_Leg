using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Zenject;
using UnityEngine;

namespace ChainSawLeg.Features.Exploration
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private GameObject collidersObject;
        [SerializeField] private SpriteRenderer[] roomSprites;
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private Color onCloseColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Transform leftUpperBound;
        [SerializeField] private Transform rightLowerBound;
        [SerializeField] private bool isStartRoom;
        [SerializeField] private UnityEvent onOpen;
        [SerializeField] private UnityEvent onClose;
        private IGameplayInputBlockService gameplayInputBlockService;
        [HideInInspector] public Bounds roomBounds;
        private CameraFlow cameraFlow;
        private Dictionary<SpriteRenderer, Color> baseSpriteColors = new Dictionary<SpriteRenderer, Color>();
        public readonly float transitionDuration = 1f;

        public void ConfigureInputBlocking(IGameplayInputBlockService inputBlockService, CameraFlow cameraFlow)
        {
            gameplayInputBlockService = inputBlockService
                ?? throw new ArgumentNullException(nameof(inputBlockService));
            roomBounds = CreateBoundsFromTransforms(leftUpperBound.position, rightLowerBound.position);
            this.cameraFlow = cameraFlow;
            foreach (var sprite in roomSprites) baseSpriteColors.Add(sprite, sprite.color);
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
                foreach (var sprite in roomSprites) sprite.color = onCloseColor;
                sortingGroup.sortingOrder--;
                collidersObject.SetActive(false);
            }
        }

        public void OpenRoom()
        {
            collidersObject.SetActive(true);
            foreach (SpriteRenderer sprite in roomSprites)
            {
                DOVirtual.Color(onCloseColor, baseSpriteColors[sprite], transitionDuration * 0.5f, (Color color) => sprite.color = color);
                sortingGroup.sortingOrder++;
            }
            DOVirtual.DelayedCall(transitionDuration * 0.5f, () => gameplayInputBlockService.ReleaseBlock(InputBlockChannels.Gameplay));
            cameraFlow.bounds = roomBounds;
            sortingGroup.enabled = false;
            onOpen?.Invoke();
        }

        public void CloseRoom(RoomManager nextRoom, Action onClose, Vector2 nextPlayerPosition)
        {
            cameraFlow.TransitToRoom(nextRoom.roomBounds, transitionDuration, nextPlayerPosition);

            gameplayInputBlockService.AcquireBlock(InputBlockChannels.Gameplay);
            
            foreach (var sprite in roomSprites)
            {
                DOVirtual.Color(baseSpriteColors[sprite], onCloseColor, transitionDuration, (Color color) => sprite.color = color);
                DOVirtual.DelayedCall(transitionDuration * 0.5f, () => { sortingGroup.sortingOrder--; });
            }
            DOVirtual.DelayedCall(transitionDuration * 0.5f, () =>
            {
                collidersObject.SetActive(false);
                sortingGroup.enabled = true;
                onClose?.Invoke();
                this.onClose?.Invoke();
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
