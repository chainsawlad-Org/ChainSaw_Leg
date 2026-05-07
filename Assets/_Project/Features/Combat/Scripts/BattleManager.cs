using UnityEngine;

public class BattleManager
{
    private readonly TurnSystem turnSystem;
    private readonly CombatResolver resolver;
    private readonly SimpleAI ai;
    private PlayerActionController playerController;

    public bool IsBattleOver
    { get; private set; }

    public BattleManager(
        TurnSystem turnSystem,
        CombatResolver resolver,
        SimpleAI ai,
        PlayerActionController playerController)
    {
        this.turnSystem = turnSystem;
        this.resolver = resolver;
        this.ai = ai;
        this.playerController = playerController;
    }

    public void Update()
    {
        if (IsBattleOver) return;

        var actor = turnSystem.GetCurrentUnit();
        var target = turnSystem.GetTarget(actor);

        // BattleAction action;

        if (actor.Id == "Player")
        {
            if (!playerController.TryGetAction(out var actionType))
                return;

            var action = new BattleAction(actor, target, actionType);
            resolver.Resolve(action);

            // // временно авто-атака
            // action = new BattleAction(actor, target, ActionType.Attack);
        }
        else
        {
            var aiAction = ai.ChooseAction();
            var action = new BattleAction(actor, target, aiAction);
            resolver.Resolve(action);
        }

        // resolver.Resolve(action);

        CheckBattleEnd();

        turnSystem.NextTurn();
    }

    private void CheckBattleEnd()
    {
        // 1х1 пока
        if (!turnSystem.GetCurrentUnit().IsAlive)
        {
            IsBattleOver = true;
            Debug.Log("Battle Ended");
            Application.Quit(); // заглушка
        }
    }
}