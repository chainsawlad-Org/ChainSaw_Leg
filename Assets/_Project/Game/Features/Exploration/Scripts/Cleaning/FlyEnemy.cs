using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Minigames
{
    public class FlyEnemy : MonoBehaviour, IKickable
    {
        [SerializeField] private Sprite[] animSprites;
        [SerializeField] private Sprite deadSprite;
        [SerializeField] private float changeSpriteRate = 0.5f;
        [SerializeField] private float speed = 4f;
        [SerializeField] private float kickForcePower = 20f;
        [SerializeField] private float stunDuration = 0.5f;
        [SerializeField] private float linearDampingAfterDeath = 5f;

        private bool isLive = true;
        private bool canMove = true;
        private int currentSpriteIndex = 0;
        private float lastChangeSpriteTime;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private CameraFlow cameraFlow;
        private Rigidbody2D playerRigidbody2D;
        private PlayerMovement playerMovement;
        private IGameplayInputBlockService inputBlockService;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        [Inject]
        private void Construct(CameraFlow cameraFlow, Rigidbody2D playerRigidbody2D, PlayerMovement playerMovement, IGameplayInputBlockService inputBlockService)
        {
            this.cameraFlow = cameraFlow;
            this.playerRigidbody2D = playerRigidbody2D;
            this.playerMovement = playerMovement;
            this.inputBlockService = inputBlockService;
        }

        private void Update()
        {
            if (!isLive) return;
            
            if (Time.time - lastChangeSpriteTime > changeSpriteRate)
            {
                currentSpriteIndex = (currentSpriteIndex + 1) % animSprites.Length;
                lastChangeSpriteTime = Time.time;
                spriteRenderer.sprite = animSprites[currentSpriteIndex];
            }
            
            if (playerRigidbody2D.position.x > rb.position.x) spriteRenderer.flipX = true;
            else spriteRenderer.flipX = false;
        }

        private void FixedUpdate()
        {
            if (!isLive || !canMove) return;
            
            Vector2 direction = playerRigidbody2D.position - rb.position;
            rb.linearVelocity = direction.normalized * speed;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!isLive) return;
            
            if (other.gameObject.CompareTag("Player"))
            {
                cameraFlow.ShakeEffect(0.3f, 100, 0.4f);
                inputBlockService.AcquireBlock(InputBlockChannels.Move);
                playerMovement.StunPlayer();
                canMove = false;
                
                Vector2 direction = playerRigidbody2D.position - rb.position;
                playerRigidbody2D.AddForce(direction.normalized * kickForcePower, ForceMode2D.Impulse);
                float basePlayerLinearDamping = playerRigidbody2D.linearDamping;
                playerRigidbody2D.linearDamping = 5f;
                
                DOVirtual.DelayedCall(stunDuration, () =>
                {
                    playerRigidbody2D.linearDamping = basePlayerLinearDamping;
                    inputBlockService.ReleaseBlock(InputBlockChannels.Move);
                    playerMovement.UnstunPlayer();
                    canMove = true;
                });
            }
        }

        public void Kick(Vector2 force)
        {
            if (isLive) Dead();
            else rb.AddForce(force * rb.mass, ForceMode2D.Impulse);
        }

        private void Dead()
        {
            isLive = false;
            spriteRenderer.sprite = deadSprite;
            rb.linearDamping = linearDampingAfterDeath;
            DOVirtual.Float(0f, 0f, 0.2f, (float x) => rb.AddForce(Vector2.down * 100000f * Time.deltaTime));
        }
    }
}
