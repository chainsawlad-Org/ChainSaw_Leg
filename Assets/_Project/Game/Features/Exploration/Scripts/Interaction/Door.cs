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
    
    public string GetInteractionPrompt() => "Press [E] to talk";

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        prevRoom.CloseRoom(() => nextRoom.OpenRoom());
        Vector3 target = new Vector3(nextRoom.transform.position.x, nextRoom.transform.position.y, Camera.main.transform.position.z);
        Camera.main.transform.DOMove(target, 1f).SetEase(Ease.InOutQuart);
    }
}