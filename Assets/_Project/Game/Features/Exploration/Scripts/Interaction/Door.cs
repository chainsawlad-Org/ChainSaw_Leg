using System;
using ChainSawLeg.Features.Exploration;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private RoomManager nextRoom;
    [SerializeField] private RoomManager prevRoom;
    [SerializeField] private Vector2 nextPlayerOffset;
    
    public string GetInteractionPrompt() => "Press [E] to talk";

    public bool CanInteract() => true;

    public void Interact(GameObject player)
    {
        if (!CanInteract())
            return;

        Vector2 nextPlayerPosition = transform.position + (Vector3)nextPlayerOffset;
        prevRoom.CloseRoom(nextRoom, () => nextRoom.OpenRoom(), nextPlayerPosition);
        if (player.TryGetComponent(out PlayerMovement movement)) movement.TransitToPosition(nextPlayerPosition, nextRoom.transitionDuration);
    }
}