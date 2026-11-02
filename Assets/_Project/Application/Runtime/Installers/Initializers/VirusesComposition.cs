using ChainSawLeg.Features.Minigames;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class VirusesComposition : IInitializable
    {
        private readonly VirusesGameManager gameManager;
        private readonly GameObject cursor;

        public VirusesComposition(
            VirusesGameManager gameManager,
            GameObject cursor)
        {
            this.gameManager = gameManager;
            this.cursor = cursor;
        }

        public void Initialize()
        {
            gameManager.Initialize(cursor);
        }
    }
}
