using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class VirusesGameManager : MonoBehaviour
    {
        private int indexLevel = 1;
        private bool levelIsStarted;

        public LevelOneController oneController;
        public LevelTwoController twoController;

        //Уровни
        public GameObject spawnOne;
        public GameObject spawnTwo;
        public GameObject spawnThree;
        public GameObject spawnFour;
        private void Update()
        {
            switch (indexLevel)
            {
                case 1:
                    if (!levelIsStarted)
                    {
                        oneController.StartLevel();
                        levelIsStarted = true;
                    }
                    break;
                case 2:
                    if (!levelIsStarted)
                    {
                        twoController.StartLevel();
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

        public void PrevLevel()
        {
            indexLevel--;
        }
    }
}
