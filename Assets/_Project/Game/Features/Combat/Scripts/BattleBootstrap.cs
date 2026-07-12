using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BattleBootstrap : MonoBehaviour
{
    [SerializeField] private HPBarView playerHPBar;
    [SerializeField] private HPBarView enemyHPBar;
    [SerializeField] private ActionTextView playerTextView;
    [SerializeField] private ActionTextView enemyTextView;
    [SerializeField] private DamageTextView playerDamageView;
    [SerializeField] private DamageTextView enemyDamageView;

    private GameStateMachine gameStateMachine;
    private PlayerActionController playerController;
    private CombatResolver resolver;
    private SimpleAI ai;
    private IRuntimeErrorLogger errorLogger;

    private BattleManager battleManager;
    private float timer;
    private bool returnedToExploration;

    [Inject]
    public void Construct(
        GameStateMachine gameStateMachine,
        PlayerActionController playerController,
        CombatResolver resolver,
        SimpleAI ai,
        IRuntimeErrorLogger errorLogger)
    {
        this.gameStateMachine = gameStateMachine;
        this.playerController = playerController;
        this.resolver = resolver;
        this.ai = ai;
        this.errorLogger = errorLogger;
    }

    private void Start()
    {

        var player = new Unit("Player", 100);
        var enemy = new Unit("Enemy", 100);

        playerHPBar.Bind(player);
        enemyHPBar.Bind(enemy);

        playerTextView.Bind(player);
        enemyTextView.Bind(enemy);

        playerDamageView.Bind(player);
        enemyDamageView.Bind(enemy);

        var playerTeam = new List<Unit> { player };
        var enemyTeam = new List<Unit> { enemy };

        var turnSystem = new TurnSystem(playerTeam, enemyTeam);
        battleManager = new BattleManager(
            turnSystem,
            resolver,
            ai,
            playerController);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < 1f)
            return;

        timer = 0f;

        battleManager?.Update();

        if (battleManager != null && battleManager.IsBattleOver)
        {
            ReturnToExplorationAsync().Forget();
        }
    }

    private async UniTask ReturnToExplorationAsync()
    {
        if (returnedToExploration)
            return;

        returnedToExploration = true;

        try
        {
            await gameStateMachine.ReplaceMain<ExplorationPhase>();
        }
        catch (Exception exception)
        {
            returnedToExploration = false;
            errorLogger.LogException(exception, nameof(BattleBootstrap));
        }
    }
}
