using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class LevelFourController : MonoBehaviour, ILevel
    {
        [SerializeField] private VirusesGameManager gameManager;
        [SerializeField] private GameObject cursor;
        [SerializeField] private GameObject mainCamera;

        [SerializeField] private GameObject spawn;
        [SerializeField] private GameObject spawnCamera;
        [SerializeField] private float scaleCamera;

        public void Initialize(GameObject curosr, GameObject mainCamera, VirusesGameManager gameManager)
        {
            this.cursor = curosr;
            this.gameManager = gameManager;
            this.mainCamera = mainCamera;
        }

        public void StartLevel()
        {
            cursor.transform.position = spawn.transform.position;
            mainCamera.transform.position = spawnCamera.transform.position;
            mainCamera.GetComponent<Camera>().orthographicSize = scaleCamera;
        }

        public void RevertLevel()
        {
            cursor.transform.position = spawn.transform.position;
        }

        public void CompleteLevel()
        {

        }
    }
}
