using ChainSawLeg.Features.Exploration;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    [RequireComponent(typeof(RoomManager))]
    public sealed class RoomManagerInputBlockAdapter : MonoBehaviour
    {
        [Inject]
        public void Construct(IGameplayInputBlockService inputBlockService, CameraFlow cameraFlow)
        {
            GetComponent<RoomManager>().ConfigureInputBlocking(inputBlockService, cameraFlow);
        }
    }
}
