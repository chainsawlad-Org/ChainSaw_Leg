using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration
{
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private RoomManager nextRoom;
        [SerializeField] private RoomManager prevRoom;
        [SerializeField] private Vector2 nextPlayerOffset;
        private PlayerMovement playerMovement;
    
        [Inject]
        public void Construct(PlayerMovement playerMovement)
        {
            this.playerMovement = playerMovement;
        }
    
        public string GetInteractionPrompt() => "Press [E] to talk";

        public bool CanInteract() => true;

        public void Interact()
        {
            if (!CanInteract())
                return;

            Vector2 nextPlayerPosition = transform.position + (Vector3)nextPlayerOffset;
            prevRoom.CloseRoom(nextRoom, () => nextRoom.OpenRoom(), nextPlayerPosition);
            playerMovement.TransitToPosition(nextPlayerPosition, nextRoom.transitionDuration);
        }
    }
}
