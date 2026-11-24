using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class LevelTwoController : MonoBehaviour, ILevel
    {
        [SerializeField] private VirusesGameManager gameManager;
        [SerializeField] private GameObject cursor;
        [SerializeField] private GameObject mainCamera;

        [SerializeField] private GameObject spawnTwo; 
        public void Initialize(GameObject curosr, GameObject mainCamera, VirusesGameManager gameManager)
        {
            this.cursor = curosr;
            this.mainCamera = mainCamera;
            this.gameManager = gameManager;
        }
        public void StartLevel()
        {
            cursor.transform.position = spawnTwo.transform.position;
            mainCamera.transform.SetParent(cursor.transform, false);
        }

        public void RevertLevel()
        {
            cursor.transform.position = spawnTwo.transform.position;
        }

        public void CompleteLevel()
        {
            gameManager.NextLevel();
        }
    }
}
