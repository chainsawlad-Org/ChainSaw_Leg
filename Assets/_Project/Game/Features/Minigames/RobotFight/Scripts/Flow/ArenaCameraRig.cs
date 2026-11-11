using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Plain-C# controller for the exploration camera during a fight: hands the camera
    /// over to the arena on entry, tracks the arena player laterally in the second phase,
    /// and restores the world camera on exit. All <c>CameraFlow</c> manipulation lives here.
    /// </summary>
    public class ArenaCameraRig
    {
        private readonly CameraFlow cameraFlow;
        private readonly Transform cameraPoint;
        private readonly Transform arenaCenterPoint;
        private readonly float followSpeed;
        private readonly float moveDuration;

        private bool followPlayer;

        /// <summary>Creates the rig. <paramref name="moveDuration"/> is the pan duration when entering the arena.</summary>
        public ArenaCameraRig(CameraFlow cameraFlow, Transform cameraPoint, Transform arenaCenterPoint,
            float followSpeed, float moveDuration)
        {
            this.cameraFlow = cameraFlow;
            this.cameraPoint = cameraPoint;
            this.arenaCenterPoint = arenaCenterPoint;
            this.followSpeed = followSpeed;
            this.moveDuration = moveDuration;
        }

        /// <summary>Detaches the camera from the world player and pans it to the arena centre.</summary>
        public void EnterArena()
        {
            cameraFlow.clampPosition = false;
            cameraPoint.position = cameraFlow.transform.position;
            cameraFlow.player = cameraPoint;
            cameraPoint.DOMove(arenaCenterPoint.position, moveDuration);
        }

        /// <summary>Reattaches the camera to the world player after the fight.</summary>
        public void ExitArena(PlayerMovement player)
        {
            cameraFlow.clampPosition = true;
            cameraFlow.transform.position = player.transform.position;
            cameraFlow.player = player.transform;
            followPlayer = false;
        }

        /// <summary>Enables lateral tracking of the arena player (second phase).</summary>
        public void EnablePlayerFollow() => followPlayer = true;

        /// <summary>Per-frame lateral tracking of the arena player; a no-op until <see cref="EnablePlayerFollow"/> is called.</summary>
        public void TrackPlayer(Vector3 playerPosition, float deltaTime)
        {
            if (!followPlayer) return;

            Vector2 direction = (playerPosition - cameraPoint.position).normalized;
            cameraPoint.Translate(Vector2.right * direction.x * deltaTime * followSpeed);
        }
    }
}
