using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BattleBootstrap : MonoBehaviour
{
    [Inject] private GameStateMachine gameStateMachine;

    public static BattleBootstrap Instance { get; private set; }

    public BattleManager Manager => battleManager;

    public HPBarView playerHPBar;
    public HPBarView enemyHPBar;
    public ActionTextView playerTextView;
    public ActionTextView enemyTextView;
    public DamageTextView playerDamageView;
    public DamageTextView enemyDamageView;

    private BattleManager battleManager;
    private float timer;
    private bool returnedToExploration;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Debug.Log("Battle Bootstrap started");

        var player = new Unit("Player", 100);
        var enemy = new Unit("Enemy", 100);

        playerHPBar.Bind(player);
        enemyHPBar.Bind(enemy);

        playerTextView.targetUnit = player;
        enemyTextView.targetUnit = enemy;

        playerDamageView.targetUnit = player;
        enemyDamageView.targetUnit = enemy;

        var playerTeam = new List<Unit> { player };
        var enemyTeam = new List<Unit> { enemy };

        var turnSystem = new TurnSystem(playerTeam, enemyTeam);
        var resolver = new CombatResolver();
        var ai = new SimpleAI();

        var controller = new PlayerActionController();
        BattleContext.PlayerController = controller;

        battleManager = new BattleManager(
            turnSystem,
            resolver,
            ai,
            controller);
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
            ReturnToExploration().Forget();
        }
    }

    private async UniTaskVoid ReturnToExploration()
    {
        if (returnedToExploration)
            return;

        returnedToExploration = true;

        Debug.Log("Return to Exploration");

        await gameStateMachine.ReplaceMain<ExplorationPhase>();
    }
}