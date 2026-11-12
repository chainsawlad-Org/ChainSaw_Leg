using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Thin facade of the robot fight orchestration. Delegates wave timing to
    /// <see cref="AttackScheduler"/>, attack execution to <see cref="AttackExecutor"/>,
    /// stage progression to <see cref="FightStageMachine"/> and the cinematics to
    /// <see cref="SecondPhaseSequence"/> / <see cref="FinaleSequence"/>. Wired entirely
    /// from serialized scene references in SC_World.
    /// </summary>
    public class AttackManager : MonoBehaviour, IRobotFightStarter
    {
        [Header("Config")]
        [SerializeField] private RobotFightConfig fightConfig;

        [Header("Links")]
        [SerializeField] private ArenaBoard arenaBoard;
        [SerializeField] private Transform itemSpawnPoint;
        [SerializeField] private PlayerMovement player;
        [SerializeField] private RobotKiller robot;
        [SerializeField] private SpriteRenderer blackBackground;
        [SerializeField] private Image whiteScreen;

        [Header("Items")]
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private GameObject twoBlocksPrefab;
        [SerializeField] private GameObject fourBlockPrefab;
        [SerializeField] private GameObject beatableBlockPrefab;

        [Header("Crowd")]
        [SerializeField] private GameObject[] peopleObjects;
        [SerializeField] private GameObject[] happyPeopleObjects;

        [Header("Waves")]
        [SerializeField] private RobotAttackConfig[] attackConfigs;

        private FightStageMachine stages;
        private AttackScheduler scheduler;
        private AttackExecutor executor;
        private SecondPhaseSequence secondPhaseSequence;
        private FinaleSequence finaleSequence;
        private IRobotFightAudio audio;
        private bool itemSpawnInHeaven;

        private void Awake()
        {
            if (fightConfig == null)
            {
                Debug.LogError("AttackManager: no RobotFightConfig asset assigned — the fight cannot start. " +
                               "Create one via Create → ChainSawLeg/Minigames/RobotFight Config and assign it to AttackManager.fightConfig.");
            }

            stages = new FightStageMachine();
            scheduler = new AttackScheduler(this);
            audio = new AudioServicePlayer();

            ItemLauncher launcher = new ItemLauncher(fightConfig, itemSpawnPoint);
            executor = new AttackExecutor(arenaBoard, robot, fightConfig, launcher, audio,
                itemPrefab, twoBlocksPrefab, fourBlockPrefab, beatableBlockPrefab,
                onBeatableKicked: scheduler.StopLooped, onBeatReturned: StartNextAttack);

            secondPhaseSequence = new SecondPhaseSequence(arenaBoard, robot, blackBackground, peopleObjects, fightConfig);
            finaleSequence = new FinaleSequence(arenaBoard, player, robot, blackBackground, whiteScreen,
                happyPeopleObjects, fightConfig, audio);
        }

        private void Start()
        {
            arenaBoard.onLastCellKicked += OnRobotKicked;
            arenaBoard.onPlayerDead += OnPlayerDead;
        }

        private void OnDestroy()
        {
            arenaBoard.onLastCellKicked -= OnRobotKicked;
            arenaBoard.onPlayerDead -= OnPlayerDead;
        }

        /// <inheritdoc cref="IRobotFightStarter.ReadyUpPlayer"/>
        public void ReadyUpPlayer(float stunDuration)
        {
            player.StunPlayer();

            DOVirtual.DelayedCall(stunDuration, () =>
            {
                DOVirtual.DelayedCall(fightConfig.PlayerUnstunDelay, () => player.UnstunPlayer());
                player.transform.DOMove(arenaBoard.GetCellPosition(arenaBoard.currentPlayerCell),
                        fightConfig.PlayerRepositionDuration).SetEase(Ease.Linear);
                arenaBoard.SetupCamera();
            });
        }

        /// <inheritdoc cref="IRobotFightStarter.StartSequence"/>
        public void StartSequence()
        {
            player.gameObject.SetActive(false);
            arenaBoard.EnablePlayer();

            scheduler.PlayScripted(attackConfigs[stages.CurrentConfigIndex],
                attack => executor.Execute(attack, itemSpawnInHeaven),
                fightConfig.ScriptedWaveEndDelay, StartNextAttack);
        }

        private void StartNextAttack()
        {
            if (stages.IsPhase1FinalWave)
            {
                StartSecondPhase();
                return;
            }

            stages.Advance();

            if (stages.IsFinale)
            {
                // The original code indexed attackConfigs[7] here and crashed (unreachable
                // with the shipped wave data); the intended progression is the finale.
                EndFight();
                return;
            }

            DOVirtual.DelayedCall(fightConfig.NextWaveDelay, StartLoopedWave);
        }

        private void StartLoopedWave()
        {
            scheduler.PlayLooped(attackConfigs[stages.CurrentConfigIndex],
                attack => executor.Execute(attack, itemSpawnInHeaven));
        }

        private void StartSecondPhase()
        {
            scheduler.StopLooped();
            arenaBoard.BeginSecondPhase();
            itemSpawnInHeaven = true;
            arenaBoard.SetActivePlayer(false);
            arenaBoard.FlipPlayerToRobot();

            secondPhaseSequence.Play(onPhaseStarted: stages.Advance, onArenaReady: StartLoopedWave);
        }

        private void OnRobotKicked()
        {
            audio.Play(AudioId.AttackHit);
            scheduler.StopLooped();
            stages.Advance();

            if (stages.IsFinale)
            {
                EndFight();
                return;
            }

            arenaBoard.SetActivePlayer(false);
            arenaBoard.SetCellDefault(arenaBoard.currentPlayerCell);

            robot.OnDamageAnimation(() =>
            {
                Vector2Int arenaSize = ArenaSizeForStage(stages.Current);

                robot.Jump(robot.transform.position + Vector3.right * fightConfig.RobotLeapDistance, () =>
                {
                    arenaBoard.CreateArenaSecondPhase(arenaSize, () =>
                    {
                        arenaBoard.SetActivePlayer(true);
                        robot.transform.position = arenaBoard.GetCellPosition(arenaBoard.beatableCell)
                            + Vector2.right * fightConfig.RobotLandingOffset;
                        StartLoopedWave();
                    });
                });
            });
        }

        private Vector2Int ArenaSizeForStage(FightStageMachine.Stage stage)
        {
            switch (stage)
            {
                case FightStageMachine.Stage.Phase2Arena2: return fightConfig.WideArenaSize;
                case FightStageMachine.Stage.Phase2Arena3: return fightConfig.WidestArenaSize;
                default: return fightConfig.SecondPhaseArenaSize;
            }
        }

        private void EndFight()
        {
            arenaBoard.SetActivePlayer(false);
            robot.OnDamageAnimation(null);
            finaleSequence.Play();
        }

        private void OnPlayerDead()
        {
            scheduler.StopLooped();
            arenaBoard.SetActivePlayer(false);
        }
    }
}
