using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class MoveChanger : MonoBehaviour
    {
        [SerializeField] Vector2 vector;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Enemy")
            {
                collision.GetComponent<Rigidbody2D>().linearVelocity = vector;
            }
        }
    }
}
