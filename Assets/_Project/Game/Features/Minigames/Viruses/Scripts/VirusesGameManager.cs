using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class VirusesGameManager : MonoBehaviour
    {
        private int indexLevel = 1;
        private bool levelIsStarted;
        private GameObject currentLevel;

        public LevelOneController oneController;
        public LevelTwoController twoController;

        //Уровни
        public GameObject spawnOne;
        public GameObject spawnTwo;
        public GameObject spawnThree;
        public GameObject spawnFour;

        public GameObject cursor;

        public void Initialize(GameObject curosr)
        {
            this.cursor = curosr;
        }
        private void Update()
        {
            switch (indexLevel)
            {
                case 1:
                    if (!levelIsStarted)
                    {
                        oneController.StartLevel();
                        currentLevel = oneController.gameObject;
                        levelIsStarted = true;
                    }
                    break;
                case 2:
                    if (!levelIsStarted)
                    {
                        twoController.StartLevel();
                        currentLevel = twoController.gameObject;
                        levelIsStarted = true;
                    }
                    break;
                case 3:
                    break;
            }
        }

        public void NextLevel()
        {
            indexLevel++;
            levelIsStarted = false;
        }

        public void LevelRevert()
        {
            currentLevel.GetComponent<ILevel>().RevertLevel();
        }
    }
}
