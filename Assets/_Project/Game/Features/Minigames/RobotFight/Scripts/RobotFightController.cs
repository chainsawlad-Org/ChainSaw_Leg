using System;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class RobotFightController : MonoBehaviour
    {
        [SerializeField] private int health = 100;
        [SerializeField] private float delayBetweenMoves = 0.25f;
        [SerializeField] private float delayBetweenKicks = 0.5f;
        [SerializeField] private float stunDuration = 0.25f;
        [SerializeField] private SpriteRenderer mistySprite;
        [SerializeField] private ArenaBoard arenaBoard;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite kickSprite;
        [SerializeField] private Sprite damagedSprite;
        [SerializeField] private CameraFlow cameraFlow;
        public float lastKickTime { get; private set;}
        
        private PlayerInputHandler inputHandler;
        private Vector2 prevMoveInput;
        private float lastMoveTime;
        private float lastDamagedTime;

        private void Awake()
        {
            inputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Start()
        {
            arenaBoard.TryMove(Vector2.up, out Vector2 newPosition);
            transform.position = newPosition;
        }

        public void ApplyDamage(int damage)
        {
            health -= damage;
            mistySprite.sprite = damagedSprite;
            cameraFlow.ShakeEffect(0.4f, 100, 0.5f);
            lastDamagedTime = Time.time;
            
            DOVirtual.DelayedCall(stunDuration, () => { mistySprite.sprite = defaultSprite; });
        }

        public void Relocate(Vector2 newPosition, float duration)
        {
            transform.DOMove(newPosition, duration).SetEase(Ease.OutQuad);
        }

        private void Update()
        {
            HandleMoving();
            HandleKick();
        }

        private void HandleMoving()
        {
            if (Time.time < lastDamagedTime + stunDuration) return;
            if (Time.time < lastKickTime + delayBetweenKicks) return;
            Vector2 moveInput = inputHandler.MoveInput;
            if (moveInput != Vector2.zero && moveInput != prevMoveInput) Move(moveInput);
            
            prevMoveInput = moveInput;
        }

        private void Move(Vector2 input)
        {
            if (Time.time < lastMoveTime + delayBetweenMoves) return;
            lastMoveTime = Time.time;

            if (arenaBoard.TryMove(input, out Vector2 newPosition))
            {
                transform.DOMove(newPosition, 0.15f);
                
                if (input.x != 0f)
                {
                    if (transform.position.x > newPosition.x) mistySprite.flipX = true;
                    else mistySprite.flipX = false;
                }
            }
        }

        private void HandleKick()
        {
            if (!inputHandler.KickPressed) return;
            inputHandler.ConsumeKick();
            if (Time.time < lastDamagedTime + stunDuration) return;
            if (Time.time < lastKickTime + delayBetweenKicks) return;
            lastKickTime = Time.time;

            Kick();
        }

        private void Kick()
        {
            cameraFlow.ShakeEffect(0.4f, 100, delayBetweenKicks);
            mistySprite.sprite = kickSprite;
            
            DOVirtual.DelayedCall(delayBetweenKicks, () => { if (mistySprite.sprite == kickSprite) mistySprite.sprite = defaultSprite; });
        }
    }
}
