using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BootstrapRunner : IBootstrapRunner
{
    private readonly GameStateMachine gameStateMachine;
    private readonly ISceneLoader sceneLoader;
    private readonly IStartupResolver startupResolver;

    public BootstrapRunner(
        GameStateMachine gameStateMachine,
        ISceneLoader sceneLoader,
        IStartupResolver startupResolver)
    {
        this.gameStateMachine = gameStateMachine;
        this.sceneLoader = sceneLoader;
        this.startupResolver = startupResolver;
    }

    public async UniTask Run(CancellationToken cancellationToken)
    {
        Debug.Log("Bootstrap: Run started");

        await sceneLoader.LoadAdditiveAsync(SceneNames.Persistent, cancellationToken);

        Type startupPhase = startupResolver.Resolve();

        if (startupPhase == typeof(ExplorationPhase))
        {
            sceneLoader.SetCurrentScene(SceneNames.World);

            await gameStateMachine.ReplaceMainAsync<ExplorationPhase>(cancellationToken);

            return;
        }

        if (startupPhase == typeof(BattlePhase))
        {
            sceneLoader.SetCurrentScene(SceneNames.Battle);

            return;
        }

        await gameStateMachine.ReplaceMainAsync<MainMenuPhase>(cancellationToken);
    }
}