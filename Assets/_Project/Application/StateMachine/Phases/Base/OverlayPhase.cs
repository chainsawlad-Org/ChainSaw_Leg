// Placement: Docs/Ru/02_ProjectStructure.md:98-116. Quote: "StateMachine знает только о жизненном цикле фаз."

using Cysharp.Threading.Tasks;

public abstract class OverlayPhase : GamePhase
{
    public virtual bool BlocksInput => true;
    public virtual bool PausesGame => false;
    public virtual bool CanStack => true;
    public virtual InputBlockChannels BlockedInputChannels =>
        BlocksInput ? InputBlockChannels.Gameplay : InputBlockChannels.None;

    public override UniTask Enter()
    {
        return UniTask.CompletedTask;
    }

    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
