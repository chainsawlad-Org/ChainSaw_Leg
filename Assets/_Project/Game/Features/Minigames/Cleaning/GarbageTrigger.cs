using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class GarbageTrigger : Garbage
    {
        [SerializeField] private ParticleSystem dustParticle;

        private CameraFlow cameraFlow;
        private EnemyFlySpawner enemySpawner;

        private bool isTriggered;

        public void Initialize(
            CameraFlow cameraFlow,
            EnemyFlySpawner enemySpawner)
        {
            this.cameraFlow = cameraFlow;
            this.enemySpawner = enemySpawner;
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
