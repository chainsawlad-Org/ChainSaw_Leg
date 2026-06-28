using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

public class GameStateMachine
{
    private readonly DiContainer container;
    private readonly Stack<OverlayPhase> overlayStack = new();

    private GamePhase currentPhase;

    public GameStateMachine(DiContainer container)
    {
        this.container = container;
    }

    public async UniTask ReplaceMain<T>() where T : SceneGamePhase
    {
        if (currentPhase != null)
        {
            await currentPhase.Exit();
        }

        currentPhase = container.Resolve<T>();

        await currentPhase.Enter();
    }

    public async UniTask PushOverlay<T>() where T : OverlayPhase
    {
        var phase = container.Resolve<T>();

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
