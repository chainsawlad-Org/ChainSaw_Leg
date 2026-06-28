using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

public class GameStateMachine
{
    private readonly IPhaseFactory phaseRegistry;
    private readonly Stack<OverlayPhase> overlayStack = new();

    private SceneGamePhase currentPhase;

    public GameStateMachine(IPhaseFactory phaseRegistry)
    {
        this.phaseRegistry = phaseRegistry;
    }

    public async UniTask ReplaceMain<T>() where T : SceneGamePhase
    {
        if (currentPhase != null)
        {
            await currentPhase.Exit();
        }

        currentPhase = phaseRegistry.Get<T>();

        await currentPhase.Enter();
    }

    public async UniTask PushOverlay<T>() where T : OverlayPhase
    {
        var phase = phaseRegistry.Get<T>();

        overlayStack.Push(phase);

        await phase.Enter();
    }

    public async UniTask PopOverlay()
    {
        if (overlayStack.Count == 0)
            return;

        var phase = overlayStack.Pop();

        await phase.Exit();
    }
}
