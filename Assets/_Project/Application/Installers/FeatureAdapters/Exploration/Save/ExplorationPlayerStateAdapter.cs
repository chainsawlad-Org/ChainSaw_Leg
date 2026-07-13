using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration.Save
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerMovement), typeof(PlayerDash))]
    public sealed class ExplorationPlayerStateAdapter : MonoBehaviour,
        IPlayerPositionProvider,
        IPlayerPositionRestorationTarget
    {
        private Rigidbody2D body;
        private PlayerMovement movement;
        private PlayerDash dash;
        private ExplorationPlayerRegistry registry;
        private InputService inputService;
        private BattleSessionService battleSessionService;

        public bool IsPlayerAvailable => body != null && isActiveAndEnabled;
        public float PositionX => body.position.x;
        public float PositionY => body.position.y;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            movement = GetComponent<PlayerMovement>();
            dash = GetComponent<PlayerDash>();
        }

        [Inject]
        public void Construct(
            ExplorationPlayerRegistry registry,
            InputService inputService,
            BattleSessionService battleSessionService)
        {
            this.registry = registry;
            this.inputService = inputService;
            this.battleSessionService = battleSessionService;
            registry.Register(this, this);

            if (battleSessionService.TryConsumeReturnPosition(out float positionX, out float positionY))
                registry.RestorePosition(positionX, positionY);
        }

        public void RestorePosition(float positionX, float positionY)
        {
            dash.ResetDashState();
            movement.ResetMovementState();
            inputService.ResetTransientInput();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = new Vector2(positionX, positionY);
            body.WakeUp();
            Physics2D.SyncTransforms();
        }

        private void OnDestroy()
        {
            registry?.Unregister(this, this);
        }
    }
}
