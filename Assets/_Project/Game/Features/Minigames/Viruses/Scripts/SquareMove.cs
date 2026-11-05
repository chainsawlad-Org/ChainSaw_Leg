using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class SquareMove : MonoBehaviour
    {
        [SerializeField] Vector2 startVector;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
        }
        void Start()
        {
            rb.linearVelocity = startVector;
        }
    }
}
