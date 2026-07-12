using System;
using System.Threading;
using Cysharp.Threading.Tasks;

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
        await sceneLoader.LoadAdditiveAsync(SceneNames.Persistent, cancellationToken);

        Type startupPhase = startupResolver.Resolve();
        await gameStateMachine.ReplaceMainAsync(startupPhase, cancellationToken);
    }
}
