using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class GameStateMachine
{
    private readonly IPhaseFactory phaseFactory;
    private readonly Stack<OverlayPhase> overlayStack = new();

    private SceneGamePhase currentPhase;

    public GameStateMachine(IPhaseFactory phaseFactory)
    {
        this.phaseFactory = phaseFactory;
    }

    public async UniTask ReplaceMain<T>() where T : SceneGamePhase
    {
        await CloseAllOverlays();

        if (currentPhase != null)
        {
            await currentPhase.Exit();
        }

        currentPhase = phaseFactory.Get<T>();

        await currentPhase.Enter();
    }

    public async UniTask PushOverlay<T>() where T : OverlayPhase
    {
        OverlayPhase phase = phaseFactory.Get<T>();

        overlayStack.Push(phase);

        await phase.Enter();
    }

    public async UniTask PopOverlay()
    {
        if (overlayStack.Count == 0)
            return;

        OverlayPhase phase = overlayStack.Pop();

        await phase.Exit();
    }

    public async UniTask CloseAllOverlays()
    {
        while (overlayStack.Count > 0)
        {
            await PopOverlay();
        }
    }

    public bool HasOverlay => overlayStack.Count > 0;
    public SceneGamePhase CurrentMainPhase => currentPhase;
}
