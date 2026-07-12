using System;
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

    public async UniTask Run()
    {
        Debug.Log("Bootstrap: Run started");

        await sceneLoader.LoadAdditive(SceneNames.Persistent);

        Type startupPhase = startupResolver.Resolve();

        if (startupPhase == typeof(ExplorationPhase))
        {
            sceneLoader.SetCurrentScene(SceneNames.World);

            await gameStateMachine.ReplaceMain<ExplorationPhase>();

            return;
        }

        if (startupPhase == typeof(BattlePhase))
        {
            sceneLoader.SetCurrentScene(SceneNames.Battle);

            return;
        }

        await gameStateMachine.ReplaceMain<MainMenuPhase>();
    }
}