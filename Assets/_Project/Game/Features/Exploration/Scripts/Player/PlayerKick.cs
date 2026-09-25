using System;
using System.IO;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration
{
    public class PlayerKick : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite kickSprite;
        [SerializeField] private Transform kickOrigin;
        [SerializeField] private float kickForcePower = 30f;
        [SerializeField] private float kickDuration = 0.5f;
        [SerializeField] private float kickCooldown = 1.5f;
        [SerializeField] private float kickRadius = 5f;
        [SerializeField] private float maxKickAngle = 90f;
        [SerializeField] private Vector2 originOffset;
        [SerializeField] private bool drawGizmos = false;
        
        private PlayerInputHandler input;
        private float lastKickTime;
        private PlayerMovement playerMovement;
        private CameraFlow cameraFlow;
        private IGameplayInputBlockService inputBlockService;
        private Sprite baseSprite;
        private Vector2 kickOriginPosition => (Vector2)kickOrigin.position + originOffset;

        private void Awake()
        {
            input = GetComponent<PlayerInputHandler>();
            playerMovement = GetComponent<PlayerMovement>();
            baseSprite = spriteRenderer.sprite;
        }

        [Inject]
        private void Construct(CameraFlow cameraFlow, IGameplayInputBlockService inputBlockService)
        {
            this.cameraFlow = cameraFlow;
            this.inputBlockService = inputBlockService;
        }

        private void Update()
        {
            HandleKickInput();
        }

        private void HandleKickInput()
        {
            if (!input.KickPressed) return;
            input.ConsumeKick();
            if (Time.time - lastKickTime < kickCooldown) return;

            Kick();
        }

        private void Kick()
        {
            lastKickTime = Time.time;
            
            inputBlockService.AcquireBlock(InputBlockChannels.Exploration);
            spriteRenderer.sprite = kickSprite;
            DOVirtual.DelayedCall(kickDuration, () =>
            {
                spriteRenderer.sprite = baseSprite;
                inputBlockService.ReleaseBlock(InputBlockChannels.Exploration);
            });
            
            Vector2 direction = playerMovement.LastMoveDir;
            Collider2D[] targets = Physics2D.OverlapCircleAll(kickOriginPosition, kickRadius);

            bool hitTarget = false;

            foreach (Collider2D target in targets)
            {
                Vector2 directionToTarget = ((Vector2)target.transform.position - kickOriginPosition).normalized;
                if (Vector2.Angle(direction, directionToTarget) > maxKickAngle) continue;
                
                if (target.TryGetComponent(out IKickable kickable))
                {
                    hitTarget = true;
                    kickable.Kick((direction + directionToTarget).normalized * kickForcePower);
                }
            }
            
            cameraFlow.ShakeEffect(hitTarget ? 0.2f : 0.075f, 100, 0.5f);
        }

        private void OnDrawGizmos()
        {
            if (drawGizmos) Gizmos.DrawWireSphere(kickOriginPosition, kickRadius);
        }
    }
}
