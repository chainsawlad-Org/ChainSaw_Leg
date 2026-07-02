
public interface IPhaseFactory
{
    T Get<T>() where T : GamePhase;
}
