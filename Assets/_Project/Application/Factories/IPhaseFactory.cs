
using System;

public interface IPhaseFactory
{
    T Get<T>() where T : GamePhase;
    GamePhase Get(Type phaseType);
}
