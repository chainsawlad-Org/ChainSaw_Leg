
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class CursorDeath : MonoBehaviour
    {

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                collision.gameObject.GetComponent<CursorContoll>().Death();
            }
        }
    }
}
