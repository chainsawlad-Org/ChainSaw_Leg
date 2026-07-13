using System.Collections.Generic;

public sealed class BattleSessionController
{
    private const int DefaultHitPoints = 100;
    private const float TurnIntervalSeconds = 1f;

    private readonly BattleManager battleManager;
    private float elapsedSeconds;

    public BattleSessionController(
        CombatResolver resolver,
        SimpleAI ai,
        PlayerActionController playerController)
    {
        Player = new Unit("Player", DefaultHitPoints);
        Enemy = new Unit("Enemy", DefaultHitPoints);

        var turnSystem = new TurnSystem(
            new List<Unit> { Player },
            new List<Unit> { Enemy });
        battleManager = new BattleManager(
            turnSystem,
            resolver,
            ai,
            playerController);
    }

    public Unit Player { get; }
    public Unit Enemy { get; }
    public bool IsBattleOver => battleManager.IsBattleOver;
    public bool IsPlayerVictory => battleManager.IsPlayerVictory;

    public void Tick(float deltaTime)
    {
        if (battleManager.IsBattleOver)
            return;

        elapsedSeconds += deltaTime;

        if (elapsedSeconds < TurnIntervalSeconds)
            return;

        elapsedSeconds = 0f;
        battleManager.Update();
    }
}
