using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class LevelThreeController : MonoBehaviour, ILevel
    {
        [SerializeField] private VirusesGameManager gameManager;
        [SerializeField] private GameObject cursor;

        [SerializeField] private GameObject spawnThree;

        public void Initialize(GameObject curosr, VirusesGameManager gameManager)
        {
            this.cursor = curosr;
            this.gameManager = gameManager;
        }

        public void StartLevel()
        {
            cursor.transform.position = spawnThree.transform.position;
        }

        public void RevertLevel()
        {

        }

        public void CompleteLevel()
        {

        }
    }
}
