using ChainSawLeg.Features.Minigames;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class VirusesComposition : IInitializable
    {
        private readonly VirusesGameManager gameManager;
        private readonly LevelTwoController levelTwoController;
        private readonly LevelThreeController levelThreeController;
        private readonly LevelFourController levelFourController;

        private readonly CursorContoll cursorContoll;
        private readonly GameObject cursor;
        private readonly GameObject mainCamera;

        public VirusesComposition(
            VirusesGameManager gameManager,
            LevelTwoController levelTwoController,
            LevelThreeController levelThreeController,
            LevelFourController levelFourController,
            [Inject(Id = "Cursor")] GameObject cursor,
            [Inject(Id = "MainCamera")] GameObject mainCamera,
            CursorContoll cursorContoll)
        {
            this.gameManager = gameManager;
            this.levelTwoController = levelTwoController;
            this.levelThreeController = levelThreeController;
            this.levelFourController = levelFourController;
            this.cursor = cursor;
            this.mainCamera = mainCamera;
            this.cursorContoll = cursorContoll;
        }

        public void Initialize()
        {
            gameManager.Initialize(cursor);
            levelTwoController.Initialize(cursor, mainCamera, gameManager);
            levelThreeController.Initialize(cursor, mainCamera, gameManager);
            levelFourController.Initialize(cursor, mainCamera, gameManager);

            cursorContoll.Initialize(gameManager);
        }
    }
}
