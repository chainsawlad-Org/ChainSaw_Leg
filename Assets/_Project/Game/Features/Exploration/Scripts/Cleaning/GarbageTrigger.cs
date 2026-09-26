using ChainSawLeg.Features.Minigames;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Features.Exploration
{
    public class GarbageTrigger : Garbage
    {
        [SerializeField] private ParticleSystem dustParticle;
        [SerializeField] private EnemyFlySpawner enemySpawner;

        private CameraFlow cameraFlow;
        private bool isTriggered;

        [Inject]
        private void Construct(CameraFlow cameraFlow)
        {
            this.cameraFlow = cameraFlow;
        }

        public override void Kick(Vector2 force)
        {
            base.Kick(force);
            
            if (isTriggered) return;
            isTriggered = true;
            
            cameraFlow.AddShakeEffect(0.1f, 100);

            DOVirtual.DelayedCall(3f, () =>
            {
                dustParticle.Play();
                cameraFlow.RemoveShakeEffect();
                cameraFlow.ShakeEffect(1f, 100, 1f);
                enemySpawner.StartSpawning();
            });
        }
    }
}
