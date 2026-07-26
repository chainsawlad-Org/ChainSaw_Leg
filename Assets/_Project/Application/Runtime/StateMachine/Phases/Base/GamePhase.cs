using Cysharp.Threading.Tasks;

public abstract class GamePhase : IGamePhase
{
    public abstract UniTask Enter();

    public abstract UniTask Exit();
}
