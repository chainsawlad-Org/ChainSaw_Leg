using System;
using ChainSawLeg.Features.Dialogue;
using ChainSawLeg.Features.Exploration;
using ChainSawLeg.Features.Exploration.Save;
using ChainSawLeg.Features.Minigames;
using ChainSawLeg.Game.Shared;
using ChainSawLeg.Infrastructure;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class WorldInstaller : MonoInstaller
    {
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private ExplorationPlayerStateAdapter playerStateAdapter;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private DialogueInputRouter dialogueInputRouter;
        [SerializeField] private ExplorationCheckpointTrigger[] checkpointTriggers;
        [SerializeField] private EnemyFlySpawner enemyFlySpawner;
        [SerializeField] private GarbageTrigger garbageTrigger;
        [SerializeField] private WorldQuestProgressionController worldQuestProgressionController;

        public override void InstallBindings()
        {
            if (cameraFlow == null)
                throw new InvalidOperationException("World camera flow is not assigned.");

            Container.Bind<CameraFlow>()
                .FromInstance(cameraFlow)
                .AsSingle();

            if (playerStateAdapter == null)
                throw new InvalidOperationException("World player state adapter is not assigned.");

            Container.Bind<ExplorationPlayerStateAdapter>()
                .FromInstance(playerStateAdapter)
                .AsSingle();

            if (dialogueInputRouter == null)
                throw new InvalidOperationException("World dialogue input router is not assigned.");

            Container.Bind<DialogueInputRouter>()
                .FromInstance(dialogueInputRouter)
                .AsSingle();

            if (playerMovement == null)
                throw new InvalidOperationException("World player movement is not assigned.");

            Container.Bind<PlayerMovement>()
                .FromInstance(playerMovement)
                .AsSingle();

            Container.Bind<PlayerInputHandler>()
                .FromComponentsInHierarchy()
                .AsCached();

            Container.BindInterfacesTo<ExplorationInputAdapterInitializer>()
                .AsSingle()
                .NonLazy();

            if (checkpointTriggers == null)
                throw new InvalidOperationException("World checkpoint triggers are not assigned.");

            foreach (ExplorationCheckpointTrigger checkpointTrigger in checkpointTriggers)
            {
                if (checkpointTrigger == null)
                    throw new InvalidOperationException("World checkpoint trigger contains a missing reference.");

                Container.Bind<ExplorationCheckpointTrigger>()
                    .FromInstance(checkpointTrigger);
            }

            if (enemyFlySpawner == null)
            {
                throw new InvalidOperationException(
                    "World enemy fly spawner is not assigned.");
            }

            if (garbageTrigger == null)
            {
                throw new InvalidOperationException(
                    "World garbage trigger is not assigned.");
            }

            Container.Bind<IGameObjectSpawner>()
                .To<ZenjectGameObjectSpawner>()
                .AsSingle();

            Container.Bind<EnemyFlySpawner>()
                .FromInstance(enemyFlySpawner)
                .AsSingle();

            Container.Bind<GarbageTrigger>()
                .FromInstance(garbageTrigger)
                .AsSingle();

            Container.BindInterfacesTo<WorldInitializer>()
                .AsSingle()
                .NonLazy();
            
            
            Container.BindInterfacesTo<DialogueInputRouterInitializer>()
                .AsSingle()
                .NonLazy();
            
            if (worldQuestProgressionController == null)
            {
                throw new InvalidOperationException(
                    "World quest progression controller is not assigned.");
            }

            Container.Bind<WorldQuestProgressionController>()
                .FromInstance(worldQuestProgressionController)
                .AsSingle();
        }
    }
}
