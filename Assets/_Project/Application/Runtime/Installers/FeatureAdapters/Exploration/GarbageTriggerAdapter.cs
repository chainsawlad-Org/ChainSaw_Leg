using ChainSawLeg.Features.Exploration;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class GarbageTriggerAdapter : MonoBehaviour
    {
        [Inject]
        private void Construct(CameraFlow cameraFlow)
        {
            GetComponent<GarbageTrigger>().Initialize(cameraFlow);
        }
    }
}
