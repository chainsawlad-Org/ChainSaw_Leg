public sealed class CombatResolver
{
    private readonly CombatEventBus eventBus;

    public CombatResolver(CombatEventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public void Resolve(BattleAction action)
    {
        eventBus.PublishActionPerformed(action.Actor, action.Type);

        switch (action.Type)
        {
            case ActionType.Attack:
                action.Target.TakeDamage(10);
                eventBus.PublishHpVisualChanged(action.Target, -10);
                break;

            case ActionType.Block:
                ApplyBlock(action.Actor);
                break;

            case ActionType.Heal:
                action.Actor.Heal(5);
                eventBus.PublishHpVisualChanged(action.Actor, 5);
                break;
        }
    }

    private void ApplyBlock(Unit unit)
    {
    }
}
