using System;
using System.Collections;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChainSawLeg.Features.Minigames
{
    public class RobotKiller : MonoBehaviour, IKickable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private HelpingDudes dudes;
        [SerializeField] private GameObject hologramsObject;
        [SerializeField] private Transform roofPoint;
        [SerializeField] private AnimationCurve flyCurve;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private AttackManager attackManager;

        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float attackCooldown = 1.5f; 
        [SerializeField] private float timeBetweenFrames = 0.05f;
        [SerializeField] private float attackRadius = 10f;
        [SerializeField] private int health = 100;
        [SerializeField] private Vector2 attackOffset;
        [SerializeField] private Vector2 kickDirection;

        private enum RobotState { Idle, Attacking, Distracted, Flying, Dead }
        private RobotState currentState = RobotState.Idle;

        private float frameTimer;
        private int currentFrameIndex;
        private bool handsDirection = true;
        private bool firstKickWasDone;
        private Coroutine throwAnimationCoroutine;

        private void Start()
        {
            // Ensure we start with the idle sprite (Index 0)
            if (sprites.Length > 0)
            {
                spriteRenderer.sprite = sprites[0];
            }
        }

        private void Update()
        {
            switch (currentState)
            {
                case RobotState.Idle:
                    HandleIdle();
                    break;
                case RobotState.Attacking:
                    HandleAttacking();
                    break;
                case RobotState.Distracted:
                    DistractedAnimation();
                    break;
            }
        }

        private void HandleIdle()
        {
            // Check distance to player
            Vector2 position = (Vector2)transform.position + attackOffset;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, attackRange);

            foreach (var collider in colliders)
            {
                if (collider.gameObject.TryGetComponent(out PlayerMovement playerMovement))
                {
                    StartAttack();
                }
            }
        }

        private void StartAttack()
        {
            currentState = RobotState.Attacking;
            frameTimer = 0f;
            currentFrameIndex = 0; // Start at idle frame
        }

        private void HandleAttacking()
        {
            frameTimer -= Time.deltaTime;

            while (frameTimer <= 0f)
            {
                AdvanceAttackAnimation();
                
                frameTimer += timeBetweenFrames; 
            }
        }

        private void AdvanceAttackAnimation()
        {
            int nextFrameIndex = handsDirection ? currentFrameIndex + 1 : currentFrameIndex - 1;
            
            if (nextFrameIndex >= sprites.Length - 1 || nextFrameIndex <= 0)
            {
                handsDirection = !handsDirection;
            }
            
            currentFrameIndex = nextFrameIndex;

            if (currentFrameIndex < sprites.Length)
            {
                spriteRenderer.sprite = sprites[currentFrameIndex];
            }

            frameTimer = timeBetweenFrames;

            if (currentFrameIndex == 0)
            {
                Hit();
                currentState = RobotState.Idle;
            }
        }

        private void Hit()
        {
            Vector2 position = (Vector2)transform.position + attackOffset;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, attackRadius);

            foreach (var collider in colliders)
            {
                if (collider.gameObject.TryGetComponent(out PlayerMovement playerMovement))
                {
                    playerMovement.StunPlayer();
                    playerMovement.GetComponent<Rigidbody2D>().AddForce(kickDirection, ForceMode2D.Impulse);
                    
                    DOVirtual.DelayedCall(0.5f, () => playerMovement.UnstunPlayer());
                    if (!firstKickWasDone) dudes.Ping();
                    firstKickWasDone = true;
                    AudioService.Play(AudioId.CartoonHammerHit);

                    break;
                }
            }
            
            cameraFlow.ShakeEffect(0.3f, 100, 0.4f);
        }

        private void DistractedAnimation()
        {
            frameTimer -= Time.deltaTime;

            if (frameTimer <= 0f)
            {
                currentFrameIndex = currentFrameIndex == 1 ? 2 : 1;
                spriteRenderer.sprite = sprites[currentFrameIndex];
                frameTimer = 0.2f;
            }
        }
        
        public void EnableHolograms()
        {
            hologramsObject.SetActive(true);
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1f);
            currentState = RobotState.Distracted;
            currentFrameIndex = 2;
        }

        public void Kick(Vector2 force)
        {
            if (currentState != RobotState.Distracted) return;

            AudioService.Play(AudioId.AttackHit);
            attackManager.ReadyUpPlayer(3f);
            currentState = RobotState.Flying;
            Sequence s = DOTween.Sequence();
            spriteRenderer.sprite = sprites[0];
            DOVirtual.DelayedCall(0.5f, () => hologramsObject.SetActive(false));
            s.Append(transform.DOShakePosition(3f, 0.4f, 100).OnComplete(() =>
            {
                AudioService.Play(AudioId.Jump);
                spriteRenderer.sprite = sprites[2];
            }));
            s.Append(transform.DOJump(roofPoint.position, 20f, 1, 1f).SetEase(flyCurve));
            s.OnComplete(() =>
            {
                AudioService.Play(AudioId.FallToGround);
                cameraFlow.ShakeEffect(0.4f, 100, 0.5f);
                DOVirtual.DelayedCall(1f, () =>
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
                    currentFrameIndex = 1;
                    DOVirtual.DelayedCall(2f, () => attackManager.StartSequence());
                });
            });
        }

        public void ApplyDamage()
        {
            health -= 40;
            transform.DOShakePosition(1f, 0.6f, 100);

            if (health <= 0)
            {
                currentState = RobotState.Dead;

                transform.DOShakePosition(3f, 0.5f, 100, 90F, false, false).OnComplete(() =>
                {
                    transform.DOJump((Vector2)transform.position + Vector2.right * 100f, 20f, 1, 8);
                });
            }
        }

        public void ThrowAnimation(Action onThrown = null, float timeBeforeThrow = 0f)
        {
            if (throwAnimationCoroutine != null) StopCoroutine(throwAnimationCoroutine);
            throwAnimationCoroutine = StartCoroutine(DoThrowAnimation(timeBeforeThrow, onThrown));
        }

        IEnumerator DoThrowAnimation(float timeBeforeThrow, Action onThrown)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                spriteRenderer.sprite = sprites[i];
                yield return new WaitForSeconds(timeBetweenFrames);
            }
            
            yield return new WaitForSeconds(timeBeforeThrow);

            for (int i = sprites.Length - 1; i >= 0; i--)
            {
                spriteRenderer.sprite = sprites[i];
                yield return new WaitForSeconds(timeBetweenFrames);
            }
            
            onThrown?.Invoke();
        }

        public void OnDamageAnimation(Action onComplete)
        {
            spriteRenderer.sprite = sprites[0];
            transform.DOShakePosition(3f, 0.65f, 100).OnComplete(() => onComplete?.Invoke());
        }

        public void PlayToSecondPhaseAnimation(Vector2 playerPosition, Action onJumped, Action onKicked)
        {
            spriteRenderer.sprite = sprites[2];
            AudioService.Play(AudioId.Jump);
            transform.DOJump(playerPosition + Vector2.right * 5f, 20f, 1, 1f)
                .SetEase(flyCurve)
                .OnComplete(() =>
                {
                    AudioService.Play(AudioId.FallToGround);
                    cameraFlow.ShakeEffect(0.3f, 100, 0.5f);
                    spriteRenderer.sprite = sprites[0];
                    onJumped?.Invoke();
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        ThrowAnimation(() =>
                        {
                            AudioService.Play(AudioId.HeavyHit);
                            onKicked?.Invoke();
                        }, 1f);
                    });
                });
        }

        public void Jump(Vector2 position, Action onComplete)
        {
            AudioService.Play(AudioId.Jump);
            spriteRenderer.sprite = sprites[2];
            transform.DOJump(position, 20f, 1, 1f).OnComplete(() =>
            {
                spriteRenderer.sprite = sprites[0];
                onComplete?.Invoke();
            });
        }

        private void OnDrawGizmos()
        {
            Vector2 position = (Vector2)transform.position + attackOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, attackRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(position, kickDirection);
        }
    }
}
