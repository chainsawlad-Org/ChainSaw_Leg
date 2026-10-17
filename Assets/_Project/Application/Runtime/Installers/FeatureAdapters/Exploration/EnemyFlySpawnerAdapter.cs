using ChainSawLeg.Features.Exploration;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class EnemyFlySpawnerAdapter : MonoBehaviour
    {
        [Inject]
        private void Construct(Rigidbody2D playerRigidbody2D, CameraFlow cameraFlow)
        {
            GetComponent<EnemyFlySpawner>().Initialize(playerRigidbody2D, cameraFlow);
        }
    }
}
