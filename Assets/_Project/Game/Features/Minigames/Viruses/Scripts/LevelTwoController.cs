using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class LevelTwoController : MonoBehaviour
    {
        [SerializeField]
        private VirusesGameManager gameManager;
        [SerializeField]
        private GameObject cursor;
        [SerializeField]
        private GameObject mainCamera;

        [SerializeField]
        private GameObject spawnTwo; 
        public void Initialize(GameObject curosr, GameObject mainCamera)
        {
            this.cursor = curosr;
            this.mainCamera = mainCamera;
        }
        public void StartLevel()
        {
            cursor.transform.position = spawnTwo.transform.position;
            mainCamera.transform.SetParent(cursor.transform, false);
        }
    }
}
