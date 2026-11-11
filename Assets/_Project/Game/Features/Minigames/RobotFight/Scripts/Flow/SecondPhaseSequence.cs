using System;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Second-phase intro cinematic: the robot jumps in next to the player, the world
    /// darkens, NPCs leave, the player is kicked aside and the new arena is built.
    /// Replaces the nested-callback block in the original <see cref="AttackManager"/>.
    /// </summary>
    public class SecondPhaseSequence
    {
        private readonly ArenaBoard arenaBoard;
        private readonly RobotKiller robot;
        private readonly SpriteRenderer blackBackground;
        private readonly GameObject[] peopleObjects;
        private readonly RobotFightConfig config;

        /// <summary>Creates the sequence. <paramref name="peopleObjects"/> are the NPCs hidden when the second phase starts.</summary>
        public SecondPhaseSequence(ArenaBoard arenaBoard, RobotKiller robot,
            SpriteRenderer blackBackground, GameObject[] peopleObjects, RobotFightConfig config)
        {
            this.arenaBoard = arenaBoard;
            this.robot = robot;
            this.blackBackground = blackBackground;
            this.peopleObjects = peopleObjects;
            this.config = config;
        }

        /// <summary>Plays the intro cinematic.</summary>
        /// <param name="onPhaseStarted">Raised when the phase rules activate (advances the fight stage).</param>
        /// <param name="onArenaReady">Raised after the new arena is built and the player can act again.</param>
        public void Play(Action onPhaseStarted, Action onArenaReady)
        {
            Vector2 playerPosition = arenaBoard.GetCellPosition(arenaBoard.currentPlayerCell);

            robot.PlayToSecondPhaseAnimation(playerPosition, onJumped: () =>
            {
                blackBackground.gameObject.SetActive(true);
                foreach (GameObject person in peopleObjects)
                {
                    person.SetActive(false);
                }

                DOVirtual.Color(blackBackground.color, Color.black, config.BackgroundFadeDuration,
                    x => blackBackground.color = x);
            }, onKicked: () =>
            {
                arenaBoard.KickPlayer(Vector2.left * config.PlayerKickForce, config.PlayerKickDuration);
                arenaBoard.EnablePlayerCameraFollow();

                onPhaseStarted?.Invoke();

                DOVirtual.DelayedCall(config.ArenaRebuildDelay, () =>
                {
                    arenaBoard.CreateArenaSecondPhase(config.SecondPhaseArenaSize, () =>
                    {
                        robot.transform.position = arenaBoard.GetCellPosition(arenaBoard.beatableCell)
                            + Vector2.right * config.RobotLandingOffset;
                        arenaBoard.SetActivePlayer(true);

                        onArenaReady?.Invoke();
                    });
                });
            });
        }
    }
}
