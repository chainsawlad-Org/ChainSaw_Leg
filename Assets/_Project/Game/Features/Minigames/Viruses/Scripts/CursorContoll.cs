using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class CursorContoll : MonoBehaviour
    {
        [SerializeField] private VirusesGameManager gameManager;
        
        public void Initialize(VirusesGameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Death()
        {
            gameManager.LevelRevert();
        }
    }
}
