using ChainSawLeg.Features.Minigames;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class VirusesComposition : IInitializable
    {
        private readonly VirusesGameManager gameManager;
        private readonly LevelTwoController levelTwoController;

        private readonly CursorContoll cursorContoll;
        private readonly GameObject cursor;
        private readonly GameObject mainCamera;

        public VirusesComposition(
            VirusesGameManager gameManager,
            LevelTwoController levelTwoController,
            [Inject(Id = "Cursor")] GameObject cursor,
            [Inject(Id = "MainCamera")] GameObject mainCamera,
            CursorContoll cursorContoll)
        {
            this.gameManager = gameManager;
            this.levelTwoController = levelTwoController;
            this.cursor = cursor;
            this.mainCamera = mainCamera;
            this.cursorContoll = cursorContoll;
        }

        public void Initialize()
        {
            gameManager.Initialize(cursor);
            levelTwoController.Initialize(cursor, mainCamera);

            cursorContoll.Initialize(gameManager);
        }
    }
}
