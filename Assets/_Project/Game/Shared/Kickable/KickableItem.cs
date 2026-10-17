using UnityEngine;

namespace ChainSawLeg.Features.Exploration
{
    public class KickableItem : MonoBehaviour, IKickable
    {
        [Tooltip("setting how much item will be independent for kicking of his rigidbody mass")]
        [SerializeField] private float massMultiplier = 1f;
        protected Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public virtual void Kick(Vector2 force)
        {
            float multiplier = rb.mass * massMultiplier;
            if (multiplier < 1f) multiplier = 1f;
            
            rb.AddForce(force * multiplier, ForceMode2D.Impulse);
        }
    }
}
