using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class ElevatorPanel : MonoBehaviour, IInteractable
{
    [SerializeField] private ColledgeManager colledgeManager;
    [SerializeField] private GameObject coridorDoor;
    [SerializeField] private GameObject hallDoor;

    private CameraFlow cameraFlow;
    private bool currentState;
    private bool isAvailable = true;
    
    [Inject]
    public void Construct(CameraFlow cameraFlow)
    {
        this.cameraFlow = cameraFlow;
    }
    
    public string GetInteractionPrompt() => "Press [E] to talk";

    public bool CanInteract() => isAvailable;

    public void Interact()
    {
        if (!CanInteract())
            return;

        isAvailable = false;
        currentState = !currentState;
        
        coridorDoor.SetActive(false);
        hallDoor.SetActive(false);
        
        cameraFlow.AddShakeEffect(0.025f, 20);
        
        colledgeManager.ChangeFloor(currentState ? 1 : 0);

        DOVirtual.DelayedCall(3f, () =>
        {
            cameraFlow.RemoveShakeEffect();
            isAvailable = true;
            
            coridorDoor.SetActive(!currentState);
            hallDoor.SetActive(currentState);
        });
    }
}
