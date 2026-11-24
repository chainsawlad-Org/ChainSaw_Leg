using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class FinishLevel : MonoBehaviour
    {
        [SerializeField] private VirusesGameManager gameManager;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                gameManager.currentLevel.GetComponent<ILevel>().CompleteLevel();
            }
        }
    }
}
