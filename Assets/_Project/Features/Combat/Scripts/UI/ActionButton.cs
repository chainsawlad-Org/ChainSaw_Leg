using UnityEngine;

public class ActionButton : MonoBehaviour
{
    public ActionType actionType;

    public void OnClick()
    {
        BattleContext.PlayerController.SelectAction(actionType);
    }
}
