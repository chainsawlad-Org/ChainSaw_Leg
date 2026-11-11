using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Fight-finale presentation: white screen fade, world restoration, robot removal
    /// and the NPCs' celebration. Replaces the nested-callback block in the original
    /// <see cref="AttackManager"/>.
    /// </summary>
    public class FinaleSequence
    {
        private static readonly Color TransparentWhite = new Color(1f, 1f, 1f, 0f);

        private readonly ArenaBoard arenaBoard;
        private readonly PlayerMovement player;
        private readonly RobotKiller robot;
        private readonly SpriteRenderer blackBackground;
        private readonly Image whiteScreen;
        private readonly GameObject[] happyPeopleObjects;
        private readonly RobotFightConfig config;
        private readonly IRobotFightAudio audio;

        /// <summary>Creates the sequence. <paramref name="happyPeopleObjects"/> are the NPCs shown at the celebration.</summary>
        public FinaleSequence(ArenaBoard arenaBoard, PlayerMovement player, RobotKiller robot,
            SpriteRenderer blackBackground, Image whiteScreen, GameObject[] happyPeopleObjects,
            RobotFightConfig config, IRobotFightAudio audio)
        {
            this.arenaBoard = arenaBoard;
            this.player = player;
            this.robot = robot;
            this.blackBackground = blackBackground;
            this.whiteScreen = whiteScreen;
            this.happyPeopleObjects = happyPeopleObjects;
            this.config = config;
            this.audio = audio;
        }

        /// <summary>Plays the finale: white fade, arena teardown, then the celebration reveal.</summary>
        public void Play()
        {
            whiteScreen.gameObject.SetActive(true);
            DOVirtual.Color(whiteScreen.color, Color.white, config.WhiteScreenFadeInDuration, x => whiteScreen.color = x)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(config.WhiteScreenHoldDuration, () =>
                    {
                        player.gameObject.SetActive(true);
                        UnityEngine.Object.Destroy(robot.gameObject);
                        arenaBoard.CloseArena(player);
                        blackBackground.gameObject.SetActive(false);

                        DOVirtual.Color(whiteScreen.color, TransparentWhite, config.WhiteScreenFadeOutDuration,
                            x => whiteScreen.color = x).SetEase(Ease.Linear);

                        audio.Play(AudioId.KidsCheering);

                        foreach (GameObject person in happyPeopleObjects)
                        {
                            person.SetActive(true);
                            person.transform.DOMove(person.transform.position + Vector3.up * config.CelebrateJumpHeight,
                                    config.CelebrateJumpDuration)
                                .SetLoops(Random.Range(config.CelebrateLoopRange.x, config.CelebrateLoopRange.y), LoopType.Yoyo)
                                .SetDelay(Random.value);
                        }
                    });
                });
        }
    }
}
