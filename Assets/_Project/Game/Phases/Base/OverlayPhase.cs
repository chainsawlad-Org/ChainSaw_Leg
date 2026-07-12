using Cysharp.Threading.Tasks;

public abstract class OverlayPhase : GamePhase
{
    public virtual bool BlocksInput => true;
    public virtual bool PausesGame => false;
    public virtual bool CanStack => true;

    public override UniTask Enter()
    {
        return UniTask.CompletedTask;
    }

    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
