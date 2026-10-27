using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class LevelTwoController : MonoBehaviour
    {
        [SerializeField]
        private VirusesGameManager gameManager;
        [SerializeField]
        private GameObject cursor;

        public void StartLevel()
        {
            cursor.transform.position = gameManager.spawnTwo.transform.position;
        }
    }
}
