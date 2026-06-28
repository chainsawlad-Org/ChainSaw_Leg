
using Cysharp.Threading.Tasks;

public abstract class OverlayPhase : GamePhase
{
    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
