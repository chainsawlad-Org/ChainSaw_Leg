using System;
using ChainSawLeg.Features.Exploration.Save;
using UnityEngine;
using Zenject;

public class WorldInstaller : MonoInstaller
{
    [SerializeField] private ExplorationPlayerStateAdapter playerStateAdapter;
    [SerializeField] private ExplorationCheckpointTrigger[] checkpointTriggers;

    public override void InstallBindings()
    {
        if (playerStateAdapter == null)
            throw new InvalidOperationException("World player state adapter is not assigned.");

        Container.Bind<ExplorationPlayerStateAdapter>()
            .FromInstance(playerStateAdapter)
            .AsSingle();

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
