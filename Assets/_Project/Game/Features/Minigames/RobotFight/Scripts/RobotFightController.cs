using System;
using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Player avatar during the robot fight. Reads the shared input handler, moves
    /// cell-to-cell via <see cref="ArenaBoard"/>, performs kicks and drives the avatar's
    /// sprite states (default / kick / damaged). Health itself is owned by
    /// <see cref="ArenaBoard"/>; this component only plays damage feedback.
    /// </summary>
    public class RobotFightController : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private ArenaBoard arenaBoard;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private SpriteRenderer mistySprite;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite kickSprite;
        [SerializeField] private Sprite damagedSprite;

        [Header("Timing (defaults reproduce the original behaviour)")]
        [Tooltip("Seconds between consecutive moves.")]
        [SerializeField] private float delayBetweenMoves = 0.25f;
        [Tooltip("Seconds between consecutive kicks; also the kick camera-shake duration.")]
        [SerializeField] private float delayBetweenKicks = 0.5f;
        [Tooltip("Seconds of input stun after taking damage.")]
        [SerializeField] private float stunDuration = 0.25f;
        [Tooltip("Seconds the cell-to-cell move tween takes.")]
        [SerializeField] private float moveDuration = 0.15f;
        [Tooltip("Camera shake strength on damage and kicks.")]
        [SerializeField] private float shakeStrength = 0.4f;
        [Tooltip("Camera shake vibrato on damage and kicks.")]
        [SerializeField] private int shakeVibrato = 100;
        [Tooltip("Camera shake duration when damaged.")]
        [SerializeField] private float damageShakeDuration = 0.5f;

        [Header("Audio")]
        [Tooltip("Volume of the move sound.")]
        [SerializeField] private float moveSoundVolume = 0.05f;
        [Tooltip("Min/max random pitch of the move sound.")]
        [SerializeField] private Vector2 moveSoundPitchRange = new Vector2(0.85f, 1.15f);

        /// <summary>Raised every time the arena player performs a kick.</summary>
        public event Action KickPerformed;

        private PlayerInputHandler inputHandler;
        private ICameraEffects cameraEffects;
        private IRobotFightAudio audio;
        private Vector2 prevMoveInput;
        private float lastMoveTime;
        private float lastKickTime;
        private float lastDamagedTime;
        private bool controlActive = true;

        private void Awake()
        {
            inputHandler = GetComponent<PlayerInputHandler>();
            cameraEffects = new CameraFlowEffects(cameraFlow);
            audio = new AudioServicePlayer();
        }

        private void Start()
        {
            transform.position = arenaBoard.GetCellPosition(arenaBoard.currentPlayerCell);
        }

        /// <summary>
        /// Plays the damage feedback (sprite swap + camera shake + input stun window).
        /// Actual HP deduction is owned by <see cref="ArenaBoard"/>.
        /// </summary>
        public void ApplyDamage(int damage)
        {
            mistySprite.sprite = damagedSprite;
            cameraEffects.Shake(shakeStrength, shakeVibrato, damageShakeDuration);
            lastDamagedTime = Time.time;

            DOVirtual.DelayedCall(stunDuration, () => { mistySprite.sprite = defaultSprite; });
        }

        /// <summary>Moves the avatar to a world position over <paramref name="duration"/> seconds.</summary>
        public void Relocate(Vector2 newPosition, float duration, Ease ease = Ease.OutQuad)
        {
            transform.DOMove(newPosition, duration).SetEase(ease);
        }

        /// <summary>Flips the avatar's facing.</summary>
        public void FlipPlayer(bool flip)
        {
            mistySprite.flipX = flip;
        }

        /// <summary>Enables or disables input handling for the arena player.</summary>
        public void SetControlActive(bool active)
        {
            controlActive = active;
        }

        private void Update()
        {
            if (!controlActive) return;

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
                transform.DOMove(newPosition, moveDuration);
                audio.Play(AudioId.Swing, moveSoundVolume, Random.Range(moveSoundPitchRange.x, moveSoundPitchRange.y));

                if (input.x != 0f)
                {
                    mistySprite.flipX = transform.position.x > newPosition.x;
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

            KickPerformed?.Invoke();
            Kick();
        }

        private void Kick()
        {
            cameraEffects.Shake(shakeStrength, shakeVibrato, delayBetweenKicks);
            mistySprite.sprite = kickSprite;

            arenaBoard.KickCell();

            DOVirtual.DelayedCall(delayBetweenKicks, () => { if (mistySprite.sprite == kickSprite) mistySprite.sprite = defaultSprite; });
        }
    }
}
