using UnityEngine;
using Zenject;

public class ActionButton : MonoBehaviour
{
    [SerializeField] private ActionType actionType;
    private PlayerActionController playerController;

    [Inject]
    public void Construct(PlayerActionController playerController)
    {
        this.playerController = playerController;
    }

    public void OnClick()
    {
        playerController.SelectAction(actionType);
    }
}
