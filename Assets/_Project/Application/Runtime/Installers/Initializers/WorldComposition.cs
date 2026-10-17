using System;
using ChainSawLeg.Features.Exploration;
using ChainSawLeg.Features.Minigames;
using ChainSawLeg.Game.Shared;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public sealed class WorldComposition : IInitializable
    {
        private readonly GarbageTrigger garbageTrigger;
        private readonly EnemyFlySpawner enemyFlySpawner;
        private readonly IGameObjectSpawner gameObjectSpawner;
        private readonly PlayerMovement playerMovement;
        private readonly CameraFlow cameraFlow;

        public WorldComposition(
            GarbageTrigger garbageTrigger,
            EnemyFlySpawner enemyFlySpawner,
            IGameObjectSpawner gameObjectSpawner,
            PlayerMovement playerMovement,
            CameraFlow cameraFlow)
        {
            this.garbageTrigger = garbageTrigger;
            this.enemyFlySpawner = enemyFlySpawner;
            this.gameObjectSpawner = gameObjectSpawner;
            this.playerMovement = playerMovement;
            this.cameraFlow = cameraFlow;
        }

        public void Initialize()
        {
            if (playerMovement == null)
            {
                throw new InvalidOperationException(
                    "PlayerMovement is not assigned.");
            }

            Rigidbody2D playerRigidbody =
                playerMovement.GetComponent<Rigidbody2D>();

            if (playerRigidbody == null)
            {
                throw new InvalidOperationException(
                    "Player Rigidbody2D is missing.");
            }

            enemyFlySpawner.Initialize(
                gameObjectSpawner,
                playerRigidbody,
                cameraFlow);

            garbageTrigger.Initialize(
                cameraFlow,
                enemyFlySpawner);
        }
    }
}