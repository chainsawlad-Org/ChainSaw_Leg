public class BattleAction
{
    public Unit Actor;
    public Unit Target;
    public ActionType Type;

    public BattleAction(Unit actor, Unit target, ActionType type)
    {
        Actor = actor;
        Target = target;
        Type = type;
    }
}