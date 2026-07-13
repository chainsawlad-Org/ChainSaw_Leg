using System;
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

    public GamePhase Get(Type phaseType)
    {
        if (phaseType == null)
            throw new ArgumentNullException(nameof(phaseType));

        if (!typeof(GamePhase).IsAssignableFrom(phaseType))
            throw new ArgumentException($"Type {phaseType.FullName} is not a game phase.", nameof(phaseType));

        return (GamePhase)container.Resolve(phaseType);
    }
}
