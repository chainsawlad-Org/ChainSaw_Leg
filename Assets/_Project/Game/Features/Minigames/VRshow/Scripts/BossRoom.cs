using System;
using ChainSawLeg.Features.Exploration;
using ChainSawLeg.Features.Minigames;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Game.Shared
{
    public class BossRoom : MonoBehaviour
    {
        [SerializeField] private float closeDuration = 1f;
        [SerializeField] private Transform doorTransform;
        [SerializeField] private Transform roomPointTransform;
        [SerializeField] private Vector2 closeDirection;
        [SerializeField] private EvilMistyBoss evilMistyBoss;
        [SerializeField] private CameraFlow cameraFlow;

        private Bounds roomBounds;

        private void Start()
        {
            roomBounds = new Bounds(roomPointTransform.position, Vector3.zero);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            cameraFlow.enabled = false;
            
            doorTransform.DOMove(doorTransform.position + (Vector3)closeDirection, closeDuration);
            DOVirtual.DelayedCall(closeDuration * 2f, () => evilMistyBoss.StartAttack()).OnComplete(() => cameraFlow.enabled = true);
            
            cameraFlow.bounds = roomBounds;
            cameraFlow.clampPosition = true;
            cameraFlow.TransitToRoom(roomBounds, closeDuration, roomPointTransform.position);
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
