using Zenject;

public class PhaseFactory : IPhaseFactory
{
    private readonly DiContainer container;

    public PhaseFactory(DiContainer container)
    {
        this.container = container;
    }

    public T Get<T>() where T : GamePhase
    {
        return container.Resolve<T>();
    }
}
