using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

public class GameStateMachine
{
    private readonly DiContainer container;
    private readonly Stack<IGamePhase> phases = new();

    public GameStateMachine(DiContainer container)
    {
        this.container = container;
    }

    public async UniTask Enter<T>() where T : IGamePhase
    {
        var phase = container.Resolve<T>();

        await Push(phase);
    }

    public async UniTask Push(IGamePhase phase)
    {
        if (phases.Count > 0)
            await phases.Peek().Exit();

        phases.Push(phase);

        await phase.Enter();
    }

    public async UniTask Pop()
    {
        if (phases.Count == 0) return;

        IGamePhase currentPhase = phases.Pop();

        await currentPhase.Exit();

        if (phases.Count > 0)
            await phases.Peek().Enter();
    }
}
