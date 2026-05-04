using UnityEngine;

public class BattleManager
{
    private readonly TurnSystem _turnSystem;
    private readonly CombatResolver _resolver;
    private readonly SimpleAI _ai;

    public bool IsBattleOver { get; private set; }

    public BattleManager(
        TurnSystem turnSystem,
        CombatResolver resolver,
        SimpleAI ai)
    {
        _turnSystem = turnSystem;
        _resolver = resolver;
        _ai = ai;
    }

    public void StartBattle()
    {
        Debug.Log("Battle Started");
    }

    public void Update()
    {
        if (IsBattleOver) return;

        var actor = _turnSystem.GetCurrentUnit();
        var target = _turnSystem.GetTarget(actor);

        BattleAction action;

        if (actor.Id == "Player")
        {
            // временно авто-атака
            action = new BattleAction(actor, target, ActionType.Attack);
        }
        else
        {
            var aiAction = _ai.ChooseAction();
            action = new BattleAction(actor, target, aiAction);
        }

        _resolver.Resolve(action);

        CheckBattleEnd();

        _turnSystem.NextTurn();
    }

    private void CheckBattleEnd()
    {
        // 1х1 пока
        if (!_turnSystem.GetCurrentUnit().IsAlive)
        {
            IsBattleOver = true;
            Debug.Log("Battle Ended");
            Application.Quit(); // заглушка
        }
    }
}