public sealed class BattleAction
{
    public Unit Actor { get; }
    public Unit Target { get; }
    public ActionType Type { get; }

    public BattleAction(Unit actor, Unit target, ActionType type)
    {
        Actor = actor;
        Target = target;
        Type = type;
    }
}
