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

        private readonly CursorContoll cursorContoll;
        private readonly GameObject cursor;
        private readonly GameObject mainCamera;

        public VirusesComposition(
            VirusesGameManager gameManager,
            LevelTwoController levelTwoController,
            LevelThreeController levelThreeController,
            [Inject(Id = "Cursor")] GameObject cursor,
            [Inject(Id = "MainCamera")] GameObject mainCamera,
            CursorContoll cursorContoll)
        {
            this.gameManager = gameManager;
            this.levelTwoController = levelTwoController;
            this.levelThreeController = levelThreeController;
            this.cursor = cursor;
            this.mainCamera = mainCamera;
            this.cursorContoll = cursorContoll;
        }

        public void Initialize()
        {
            gameManager.Initialize(cursor);
            levelTwoController.Initialize(cursor, mainCamera, gameManager);
            levelThreeController.Initialize(cursor, gameManager);

            cursorContoll.Initialize(gameManager);
        }
    }
}
