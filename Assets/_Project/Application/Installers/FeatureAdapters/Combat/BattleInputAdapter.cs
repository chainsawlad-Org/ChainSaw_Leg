using UnityEngine;
using Zenject;

public class BattleInputAdapter : MonoBehaviour
{
    private InputService inputService;
    private PlayerActionController playerController;

    [Inject]
    public void Construct(
        InputService inputService,
        PlayerActionController playerController)
    {
        this.inputService = inputService;
        this.playerController = playerController;
    }

    private void Update()
    {
        if (inputService == null)
            return;

        if (inputService.SubmitPressed)
        {
            playerController.SelectAction(ActionType.Attack);
            inputService.ConsumeSubmit();
        }

        if (inputService.InteractPressed)
        {
            playerController.SelectAction(ActionType.Heal);
            inputService.ConsumeInteract();
        }

        if (inputService.DashPressed)
        {
            playerController.SelectAction(ActionType.Block);
            inputService.ConsumeDash();
        }
    }
}
