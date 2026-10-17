using System;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class Garbage : KickableItem
    {
        [SerializeField] private bool isFreezed;
        [SerializeField] private Vector2 targetDeltaPoint;
        [SerializeField] private float jumpPower;
        [SerializeField] private float animationDuration;

        private void Start()
        {
            if (isFreezed) rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public override void Kick(Vector2 force)
        {
            base.Kick(force);
            if (isFreezed)
            {
                rb.constraints = RigidbodyConstraints2D.None;
                transform.DOJump(transform.position + (Vector3)targetDeltaPoint, jumpPower, 1, animationDuration).SetEase(Ease.Linear);
                isFreezed = false;
            }
        }
    }
}
