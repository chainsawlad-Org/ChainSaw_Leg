using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class GameStateMachine
{
    private readonly Stack<IGamePhase> phases = new();

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
