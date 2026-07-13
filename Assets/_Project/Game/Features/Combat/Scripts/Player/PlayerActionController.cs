using System;
using UnityEngine;

public class PlayerActionController
{
    private ActionType? selectedAction;

    public void SelectAction(ActionType action)
    {
        selectedAction = action;
    }

    public bool TryGetAction(out ActionType action)
    {
        if (selectedAction.HasValue)
        {
            action = selectedAction.Value;
            selectedAction = null;
            return true;
        }

        action = default;
        return false;
    }
}
