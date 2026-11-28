using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class VirusesGameManager : MonoBehaviour
    {
        private int indexLevel = 1;
        private bool levelIsStarted;
        public GameObject currentLevel;

        public LevelOneController oneController;
        public LevelTwoController twoController;
        public LevelThreeController threeController;
        public LevelFourController fourController;

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
                        currentLevel = twoController.gameObject;
                        currentLevel.GetComponent<ILevel>().StartLevel();
                        levelIsStarted = true;
                    }
                    break;
                case 3:
                    if (!levelIsStarted)
                    {
                        currentLevel = threeController.gameObject;
                        currentLevel.GetComponent<ILevel>().StartLevel();
                        levelIsStarted = true;
                    }
                    break;
                case 4:
                    if (!levelIsStarted)
                    {
                        currentLevel = fourController.gameObject;
                        currentLevel.GetComponent<ILevel>().StartLevel();
                        levelIsStarted = true;
                    }
                    break;
            }
        }

        public void NextLevel()
        {
            indexLevel++;
            levelIsStarted = false;
            Debug.Log("Next level " +  indexLevel);
        }

        public void LevelRevert()
        {
            currentLevel.GetComponent<ILevel>().RevertLevel();
        }
    }
}
