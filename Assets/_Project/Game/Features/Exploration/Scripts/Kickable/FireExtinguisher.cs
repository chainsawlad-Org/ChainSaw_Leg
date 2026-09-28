using System;
using DG.Tweening;
using UnityEngine;

namespace ChainSawLeg.Features.Exploration
{
    public class FireExtinguisher : KickableItem
    {
        [SerializeField] private float explosionDuration = 5f;
        [SerializeField] private float force = 100f;
        [SerializeField] private float torque = 20f;
        [SerializeField] private float impulseFromWalls = 10f;
        [SerializeField] private float delayBeforeExplose = 2f;
        [SerializeField] private ParticleSystem particle;

        private bool wasKicked;
        private float prevExplosionState;
        private float baseLinearDamping;

        private void Start()
        {
            baseLinearDamping = rb.linearDamping;
        }

        public override void Kick(Vector2 force)
        {
            base.Kick(force);
            
            DOVirtual.DelayedCall(delayBeforeExplose, () =>
            {
                if (explosionDuration > 0f)
                {
                    rb.linearDamping = 0f;
                    particle.Play();
                }
                
                wasKicked = true;
            });
        }

        private void FixedUpdate()
        {
            if (!wasKicked) return;
            if (explosionDuration <= 0f) return;
            prevExplosionState = explosionDuration;
            explosionDuration -= Time.fixedDeltaTime;
            
            if (prevExplosionState > 0f && explosionDuration <= 0f)
            {
                rb.linearDamping = baseLinearDamping;
                particle.Stop();
            }
            
            rb.AddForce(-transform.up * force);
            rb.AddTorque(torque);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (explosionDuration <= 0f || !wasKicked) return;

            Vector2 direction = transform.position - (Vector3)other.GetContact(0).point;
            rb.AddForce(direction * impulseFromWalls, ForceMode2D.Impulse);
        }
    }
}
