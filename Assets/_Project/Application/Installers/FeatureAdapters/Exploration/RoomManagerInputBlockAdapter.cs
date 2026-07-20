using ChainSawLeg.Features.Exploration;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(RoomManager))]
public sealed class RoomManagerInputBlockAdapter : MonoBehaviour
{
    [Inject]
    public void Construct(IGameplayInputBlockService inputBlockService)
    {
        GetComponent<RoomManager>().ConfigureInputBlocking(inputBlockService);
    }
}
