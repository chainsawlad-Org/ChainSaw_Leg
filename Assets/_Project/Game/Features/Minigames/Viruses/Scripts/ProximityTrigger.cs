using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class ProximityTrigger : MonoBehaviour
    {
        [SerializeField] private RotationSquare rotationSquare;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                rotationSquare.Proximity();
            }
        }
    }
}
