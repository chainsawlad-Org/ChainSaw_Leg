using System;
using ChainSawLeg.Features.Exploration.Save;
using UnityEngine;
using Zenject;

public class WorldInstaller : MonoInstaller
{
    [SerializeField] private CameraFlow cameraFlow;
    [SerializeField] private ExplorationPlayerStateAdapter playerStateAdapter;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private DialogueInputRouter dialogueInputRouter;
    [SerializeField] private ExplorationCheckpointTrigger[] checkpointTriggers;

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
            .FromComponentInHierarchy()
            .AsSingle();

        Container.BindInterfacesTo<ExplorationInputAdapterInitializer>()
            .AsSingle()
            .NonLazy();

        Container.Bind<DialogueManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.BindInterfacesTo<DialogueRuntimeAdapterInitializer>()
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
    }
}
