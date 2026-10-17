using System;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Exploration
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
        private Rigidbody2D playerRigidbody2D;
        private PlayerMovement playerMovement;
        private CameraFlow cameraFlow;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Rigidbody2D playerRigidbody2D, CameraFlow cameraFlow)
        {
            this.playerRigidbody2D = playerRigidbody2D;
            this.cameraFlow = cameraFlow;
            playerMovement = playerRigidbody2D.GetComponent<PlayerMovement>();
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
            
            if (other.gameObject.TryGetComponent(out PlayerMovement player))
            {
                cameraFlow.ShakeEffect(0.3f, 100, 0.4f);
                playerMovement.StunPlayer();
                canMove = false;
                
                Vector2 direction = playerRigidbody2D.position - rb.position;
                playerRigidbody2D.AddForce(direction.normalized * kickForcePower, ForceMode2D.Impulse);
                float basePlayerLinearDamping = playerRigidbody2D.linearDamping;
                playerRigidbody2D.linearDamping = 5f;
                
                DOVirtual.DelayedCall(stunDuration, () =>
                {
                    playerRigidbody2D.linearDamping = basePlayerLinearDamping;
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
