using ChainSawLeg.Features.Exploration;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private RoomManager nextRoom;
    [SerializeField] private RoomManager prevRoom;

    public string GetInteractionPrompt() => "Press [E] to talk";

    public bool CanInteract() => true;

    public void Interact()
    {
        if (!CanInteract())
            return;

        prevRoom.CloseRoom(nextRoom.transform.position, () => nextRoom.OpenRoom());
    }
}
