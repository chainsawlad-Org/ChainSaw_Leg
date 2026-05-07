
using System;

public static class BattleEvents
{
    public static Action<Unit, ActionType> OnActionPerfomed;
    public static Action<Unit, int> OnHPChangedVisual;
}
