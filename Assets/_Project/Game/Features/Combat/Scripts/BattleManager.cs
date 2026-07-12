using UnityEngine;

public class BattleManager
{
    private readonly TurnSystem turnSystem;
    private readonly CombatResolver resolver;
    private readonly SimpleAI ai;
    private readonly PlayerActionController playerController;

    public bool IsBattleOver { get; private set; }

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
        if (IsBattleOver)
            return;

        var actor = turnSystem.GetCurrentUnit();
        var target = turnSystem.GetTarget(actor);

        if (actor.Id == "Player")
        {
            if (!playerController.TryGetAction(out var actionType))
                return;

            var action = new BattleAction(actor, target, actionType);
            resolver.Resolve(action);
        }
        else
        {
            var aiAction = ai.ChooseAction();
            var action = new BattleAction(actor, target, aiAction);
            resolver.Resolve(action);
        }

        CheckBattleEnd();

        if (!IsBattleOver)
        {
            turnSystem.NextTurn();
        }
    }

    private void CheckBattleEnd()
    {
        // Пока 1x1
        if (!turnSystem.GetCurrentUnit().IsAlive)
        {

            IsBattleOver = true;
        }
    }
}
