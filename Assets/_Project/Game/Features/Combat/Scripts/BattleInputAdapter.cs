using UnityEngine;

public class BattleInputAdapter : MonoBehaviour
{
    public PlayerInputHandler playerInput;

    void Update()
    {
        var controller = BattleContext.PlayerController;

        if (playerInput.SubmitPressed)
        {
            controller.SelectAction(ActionType.Attack);
            playerInput.ConsumeSubmit();
        }

        if (playerInput.InteractPressed)
        {
            controller.SelectAction(ActionType.Heal);
            playerInput.ConsumeInteract();
        }

        if (playerInput.DashPressed)
        {
            controller.SelectAction(ActionType.Block);
            playerInput.ConsumeDash();
        }
    }
}
