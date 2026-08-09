using System;
using System.Collections.Generic;
public class StartupPhaseRegistry
{
    private readonly Dictionary<string, Type> phases = new();

    public void Register<TPhase>(string sceneName) where TPhase : SceneGamePhase
    {
        phases[sceneName] = typeof(TPhase);
    }

    public bool TryGet(string sceneName, out Type phase)
    {
        return phases.TryGetValue(sceneName, out phase);
    }
}
