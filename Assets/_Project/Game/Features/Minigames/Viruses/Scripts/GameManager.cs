using log4net.Core;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class GameManager : MonoBehaviour
    {
        private int indexLevel = 1;
        private bool levelIsStarted;
        public LevelOneController oneController;

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
                    break;
                case 3:
                    break;
            }
        }

        public void NextLevel()
        {
            indexLevel++;
        }

        public void PrevLevel()
        {
            indexLevel--;
        }
    }
}
