using UnityEngine;
using Zenject;

public class BattleInputAdapter : MonoBehaviour
{
    private InputService inputService;

    [Inject]
    public void Construct(InputService inputService)
    {
        this.inputService = inputService;
    }

    private void Update()
    {
        var controller = BattleContext.PlayerController;

        if (controller == null || inputService == null)
            return;

        if (inputService.SubmitPressed)
        {
            controller.SelectAction(ActionType.Attack);
            inputService.ConsumeSubmit();
        }

        if (inputService.InteractPressed)
        {
            controller.SelectAction(ActionType.Heal);
            inputService.ConsumeInteract();
        }

        if (inputService.DashPressed)
        {
            controller.SelectAction(ActionType.Block);
            inputService.ConsumeDash();
        }
    }
}
