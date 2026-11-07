using System;
using ChainSawLeg.Features.Exploration;
using UnityEngine;

namespace ChainSawLeg.Game.Shared
{
    public class TrashPaper : MonoBehaviour, IKickable
    {
        [SerializeField] private ParticleSystem particle;

        public void Kick(Vector2 force)
        {
            particle.transform.parent = null;
            
            Vector2 direction = force * 2f;
            
            Vector2 rotatedLeft = Quaternion.Euler(0, 0, -33) * direction * 0.1f;
            Vector2 rotatedRight = Quaternion.Euler(0, 0, 33) * direction;
            
            ParticleSystem.VelocityOverLifetimeModule velocity = particle.velocityOverLifetime;
            velocity.x = new ParticleSystem.MinMaxCurve(rotatedLeft.x, rotatedRight.x);
            velocity.y = new ParticleSystem.MinMaxCurve(rotatedLeft.y, rotatedRight.y);
            
            particle.Play();
            
            Destroy(gameObject);
        }
    }
}
