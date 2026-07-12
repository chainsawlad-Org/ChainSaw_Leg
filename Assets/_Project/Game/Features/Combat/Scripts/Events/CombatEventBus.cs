
using System;

public sealed class CombatEventBus
{
    public event Action<Unit, ActionType> ActionPerformed;
    public event Action<Unit, int> HpVisualChanged;

    public void PublishActionPerformed(Unit unit, ActionType actionType)
    {
        ActionPerformed?.Invoke(unit, actionType);
    }

    public void PublishHpVisualChanged(Unit unit, int delta)
    {
        HpVisualChanged?.Invoke(unit, delta);
    }
}
