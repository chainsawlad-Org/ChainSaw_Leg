using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class BootstrapRunner : IBootstrapRunner
{
    private readonly GameStateMachine gameStateMachine;
    private readonly ISceneLoader sceneLoader;
    private readonly IStartupResolver startupResolver;
    private readonly IReadOnlyList<IApplicationService> applicationServices;

    public BootstrapRunner(
        GameStateMachine gameStateMachine,
        ISceneLoader sceneLoader,
        IStartupResolver startupResolver,
        List<IApplicationService> applicationServices)
    {
        this.gameStateMachine = gameStateMachine;
        this.sceneLoader = sceneLoader;
        this.startupResolver = startupResolver;
        this.applicationServices = applicationServices;
    }

    public async UniTask Run(CancellationToken cancellationToken)
    {
        foreach (IApplicationService service in applicationServices)
            await service.Initialize();

        await sceneLoader.LoadAdditiveAsync(SceneNames.Persistent, cancellationToken);

        Type startupPhase = startupResolver.Resolve();
        await gameStateMachine.ReplaceMainAsync(startupPhase, cancellationToken);
    }
}
