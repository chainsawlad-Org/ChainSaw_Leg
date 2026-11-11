using System;
using System.Collections;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// The enemy robot. Owns its state machine (idle / attacking / distracted / flying /
    /// dead), combat detection against the world player and its cinematic reactions;
    /// sprite-frame animation is delegated to <see cref="SpriteFrameAnimator"/>, camera
    /// and audio go through adapters, and the fight is started via <see cref="IRobotFightStarter"/>
    /// (no direct dependency on the orchestrator's type).
    /// </summary>
    public class RobotKiller : MonoBehaviour, IKickable
    {
        [Header("Links")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private HelpingDudes dudes;
        [SerializeField] private GameObject hologramsObject;
        [SerializeField] private Transform roofPoint;
        [SerializeField] private AnimationCurve flyCurve;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private AttackManager attackManager;

        [Header("Combat")]
        [Tooltip("Radius at which the robot starts attacking the approaching world player.")]
        [SerializeField] private float attackRange = 3f;
        [Tooltip("Radius of the hammer hit applied when the attack animation wraps around; may exceed the detection range.")]
        [SerializeField] private float attackRadius = 10f;
        [Tooltip("Offset of the detection and hit circles from the robot's origin.")]
        [SerializeField] private Vector2 attackOffset;
        [Tooltip("Impulse applied to the world player on a hammer hit.")]
        [SerializeField] private Vector2 kickDirection;
        [Tooltip("Damage the robot takes from one kicked-back block.")]
        [SerializeField] private int damagePerHit = 40;
        [Tooltip("Robot health; the robot is defeated (death launch) when it reaches zero.")]
        [SerializeField] private int health = 100;
        [Tooltip("Seconds the world player stays stunned after a hammer hit.")]
        [SerializeField] private float hitStunDuration = 0.5f;
        [Tooltip("Stun duration passed to the fight when the robot is kicked into the arena.")]
        [SerializeField] private float readyUpStun = 3f;

        [Header("Frame Animation")]
        [Tooltip("Seconds between sprite frames of the throw animation.")]
        [SerializeField] private float timeBetweenFrames = 0.05f;
        [Tooltip("Seconds per frame of the distracted (hologram) toggle animation.")]
        [SerializeField] private float distractedFrameTime = 0.2f;
        [Tooltip("Index of the idle pose. Reordering the sprites array breaks these indices.")]
        [SerializeField] private int idleFrameIndex = 0;
        [Tooltip("First frame of the distracted toggle animation (original sprites[1]).")]
        [SerializeField] private int distractFrameIndexA = 1;
        [Tooltip("Leap pose; also the second frame of the distracted toggle animation (original sprites[2]).")]
        [SerializeField] private int leapFrameIndex = 2;

        [Header("Hammer Hit Feedback")]
        [SerializeField] private float hitShakeStrength = 0.3f;
        [SerializeField] private int hitShakeVibrato = 100;
        [SerializeField] private float hitShakeDuration = 0.4f;

        [Header("Kick Intro Cinematic (defaults reproduce the original behaviour)")]
        [SerializeField] private float kickShakeDuration = 3f;
        [SerializeField] private float kickShakeStrength = 0.4f;
        [SerializeField] private int kickShakeVibrato = 100;
        [Tooltip("Seconds after the kick before the holograms disappear.")]
        [SerializeField] private float hologramHideDelay = 0.5f;
        [SerializeField] private float jumpHeight = 20f;
        [SerializeField] private float jumpDuration = 1f;
        [SerializeField] private int landShakeVibrato = 100;
        [SerializeField] private float landShakeDuration = 0.5f;
        [SerializeField] private float kickLandShakeStrength = 0.4f;
        [Tooltip("Seconds between the roof landing and the pose flip back.")]
        [SerializeField] private float landRecoveryDelay = 1f;
        [Tooltip("Seconds between the pose flip back and the fight start.")]
        [SerializeField] private float fightStartDelay = 2f;

        [Header("Second Phase Cinematic")]
        [SerializeField] private float secondPhaseLandShakeStrength = 0.3f;
        [Tooltip("Seconds between the second-phase landing and the heavy-hit throw.")]
        [SerializeField] private float secondPhaseThrowDelay = 2f;
        [Tooltip("Seconds between the heavy-hit throw wind-up and the impact.")]
        [SerializeField] private float heavyHitThrowDelay = 1f;
        [Tooltip("Horizontal offset the robot jumps to, next to the player.")]
        [SerializeField] private float jumpInOffset = 5f;

        [Header("Damage & Death")]
        [SerializeField] private float damageShakeDuration = 1f;
        [SerializeField] private float damageShakeStrength = 0.6f;
        [SerializeField] private int damageShakeVibrato = 100;
        [Tooltip("Kicked-back-block reaction shake duration.")]
        [SerializeField] private float onDamageShakeDuration = 3f;
        [Tooltip("Kicked-back-block reaction shake strength.")]
        [SerializeField] private float onDamageShakeStrength = 0.65f;
        [Tooltip("Kicked-back-block reaction shake vibrato.")]
        [SerializeField] private int onDamageShakeVibrato = 100;
        [SerializeField] private float deathShakeDuration = 3f;
        [SerializeField] private float deathShakeStrength = 0.5f;
        [SerializeField] private int deathShakeVibrato = 100;
        [SerializeField] private float deathShakeRandomness = 90f;
        [Tooltip("Distance the defeated robot is launched sideways.")]
        [SerializeField] private float deathLaunchDistance = 100f;
        [Tooltip("Seconds the defeated robot's final launch jump takes.")]
        [SerializeField] private float deathJumpDuration = 8f;

        private enum RobotState { Idle, Attacking, Distracted, Flying, Dead }

        private RobotState currentState = RobotState.Idle;
        private SpriteFrameAnimator frames;
        private ICameraEffects cameraEffects;
        private IRobotFightAudio audio;
        private IRobotFightStarter fightStarter;
        private float frameTimer;
        private bool firstKickWasDone;
        private Coroutine throwAnimationCoroutine;

        private void Awake()
        {
            frames = new SpriteFrameAnimator(sprites, spriteRenderer, idleFrameIndex);
            cameraEffects = new CameraFlowEffects(cameraFlow);
            audio = new AudioServicePlayer();
            fightStarter = attackManager;
        }

        private void Start()
        {
            dudes.Interacted += EnableHolograms;

            // Ensure we start with the idle pose.
            if (sprites.Length > 0)
            {
                frames.ShowFrame(idleFrameIndex);
            }
        }

        private void OnDestroy()
        {
            dudes.Interacted -= EnableHolograms;
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
                    HandleDistracted();
                    break;
            }
        }

        private void HandleIdle()
        {
            Vector2 position = (Vector2)transform.position + attackOffset;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, attackRange);

            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject.TryGetComponent(out PlayerMovement _))
                {
                    StartAttack();
                    break;
                }
            }
        }

        private void StartAttack()
        {
            currentState = RobotState.Attacking;
            frameTimer = 0f;
        }

        private void HandleAttacking()
        {
            frameTimer -= Time.deltaTime;
            if (frameTimer > 0f) return;

            bool wrappedToIdle = frames.StepPingPong();

            // The original advanced the frame timer twice per step (a reset inside the
            // step plus an extra interval in the loop), making the effective frame
            // period twice timeBetweenFrames — preserved exactly here.
            frameTimer = timeBetweenFrames * 2f;

            if (wrappedToIdle)
            {
                Hit();
                currentState = RobotState.Idle;
            }
        }

        private void HandleDistracted()
        {
            frameTimer -= Time.deltaTime;

            if (frameTimer <= 0f)
            {
                frames.Toggle(distractFrameIndexA, leapFrameIndex);
                frameTimer = distractedFrameTime;
            }
        }

        private void Hit()
        {
            Vector2 position = (Vector2)transform.position + attackOffset;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, attackRadius);

            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject.TryGetComponent(out PlayerMovement playerMovement))
                {
                    playerMovement.StunPlayer();
                    playerMovement.GetComponent<Rigidbody2D>().AddForce(kickDirection, ForceMode2D.Impulse);

                    DOVirtual.DelayedCall(hitStunDuration, () => playerMovement.UnstunPlayer());
                    if (!firstKickWasDone) dudes.Ping();
                    firstKickWasDone = true;
                    audio.Play(AudioId.CartoonHammerHit);

                    break;
                }
            }

            cameraEffects.Shake(hitShakeStrength, hitShakeVibrato, hitShakeDuration);
        }

        /// <summary>Called by the dudes' interaction: the holograms distract the robot, allowing it to be kicked.</summary>
        public void EnableHolograms()
        {
            hologramsObject.SetActive(true);
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1f);
            currentState = RobotState.Distracted;
            frames.PrimeFrame(leapFrameIndex);
        }

        /// <inheritdoc cref="IKickable.Kick"/>
        public void Kick(Vector2 force)
        {
            if (currentState != RobotState.Distracted) return;

            audio.Play(AudioId.AttackHit);
            fightStarter.ReadyUpPlayer(readyUpStun);
            currentState = RobotState.Flying;
            frames.ShowFrame(idleFrameIndex);
            DOVirtual.DelayedCall(hologramHideDelay, () => hologramsObject.SetActive(false));

            Sequence kickSequence = DOTween.Sequence();
            kickSequence.Append(transform.DOShakePosition(kickShakeDuration, kickShakeStrength, kickShakeVibrato)
                .OnComplete(() =>
                {
                    audio.Play(AudioId.Jump);
                    frames.ShowFrame(leapFrameIndex);
                }));
            kickSequence.Append(transform.DOJump(roofPoint.position, jumpHeight, 1, jumpDuration).SetEase(flyCurve));
            kickSequence.OnComplete(() =>
            {
                audio.Play(AudioId.FallToGround);
                cameraEffects.Shake(kickLandShakeStrength, landShakeVibrato, landShakeDuration);
                DOVirtual.DelayedCall(landRecoveryDelay, () =>
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
                    frames.PrimeFrame(distractFrameIndexA);
                    DOVirtual.DelayedCall(fightStartDelay, () => fightStarter.StartSequence());
                });
            });
        }

        /// <summary>Applies damage from one kicked-back block; triggers the death launch when health runs out.</summary>
        public void ApplyDamage()
        {
            health -= damagePerHit;
            transform.DOShakePosition(damageShakeDuration, damageShakeStrength, damageShakeVibrato);

            if (health <= 0)
            {
                currentState = RobotState.Dead;

                transform.DOShakePosition(deathShakeDuration, deathShakeStrength, deathShakeVibrato, deathShakeRandomness, false, false)
                    .OnComplete(() =>
                    {
                        transform.DOJump((Vector2)transform.position + Vector2.right * deathLaunchDistance, jumpHeight, 1, deathJumpDuration);
                    });
            }
        }

        /// <summary>Plays the throw animation (frames forward, pause, frames back), then invokes <paramref name="onThrown"/>.</summary>
        /// <param name="onThrown">Raised when the item is released (after the frames rewind).</param>
        /// <param name="timeBeforeThrow">Seconds to hold the raised pose before <paramref name="onThrown"/>.</param>
        public void ThrowAnimation(Action onThrown = null, float timeBeforeThrow = 0f)
        {
            if (throwAnimationCoroutine != null) StopCoroutine(throwAnimationCoroutine);
            throwAnimationCoroutine = StartCoroutine(DoThrowAnimation(timeBeforeThrow, onThrown));
        }

        private IEnumerator DoThrowAnimation(float timeBeforeThrow, Action onThrown)
        {
            for (int i = 0; i < frames.FrameCount; i++)
            {
                frames.ShowFrame(i);
                yield return new WaitForSeconds(timeBetweenFrames);
            }

            yield return new WaitForSeconds(timeBeforeThrow);

            for (int i = frames.FrameCount - 1; i >= 0; i--)
            {
                frames.ShowFrame(i);
                yield return new WaitForSeconds(timeBetweenFrames);
            }

            onThrown?.Invoke();
        }

        /// <summary>Reaction to a kicked-back block hitting the robot; <paramref name="onComplete"/> drives the fight progression.</summary>
        public void OnDamageAnimation(Action onComplete)
        {
            frames.ShowFrame(idleFrameIndex);
            transform.DOShakePosition(onDamageShakeDuration, onDamageShakeStrength, onDamageShakeVibrato)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>Second-phase intro: jumps next to the player, lands, then performs the heavy-hit throw.</summary>
        /// <param name="onJumped">Raised at the moment of landing.</param>
        /// <param name="onKicked">Raised when the heavy-hit throw impacts the player.</param>
        public void PlayToSecondPhaseAnimation(Vector2 playerPosition, Action onJumped, Action onKicked)
        {
            frames.ShowFrame(leapFrameIndex);
            audio.Play(AudioId.Jump);
            transform.DOJump(playerPosition + Vector2.right * jumpInOffset, jumpHeight, 1, jumpDuration)
                .SetEase(flyCurve)
                .OnComplete(() =>
                {
                    audio.Play(AudioId.FallToGround);
                    cameraEffects.Shake(secondPhaseLandShakeStrength, landShakeVibrato, landShakeDuration);
                    frames.ShowFrame(idleFrameIndex);
                    onJumped?.Invoke();
                    DOVirtual.DelayedCall(secondPhaseThrowDelay, () =>
                    {
                        ThrowAnimation(() =>
                        {
                            audio.Play(AudioId.HeavyHit);
                            onKicked?.Invoke();
                        }, heavyHitThrowDelay);
                    });
                });
        }

        /// <summary>Leaps to a position; used between second-phase arenas.</summary>
        public void Jump(Vector2 position, Action onComplete)
        {
            audio.Play(AudioId.Jump);
            frames.ShowFrame(leapFrameIndex);
            transform.DOJump(position, jumpHeight, 1, jumpDuration).OnComplete(() =>
            {
                frames.ShowFrame(idleFrameIndex);
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
