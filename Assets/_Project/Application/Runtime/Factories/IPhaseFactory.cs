using System;

namespace ChainSawLeg.Application
{
    public interface IPhaseFactory
    {
        T Get<T>() where T : GamePhase;
        GamePhase Get(Type phaseType);
    }
}
