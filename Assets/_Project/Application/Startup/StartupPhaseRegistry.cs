using System;
using System.Collections.Generic;
public class StartupPhaseRegistry
{
    private readonly Dictionary<string, Type> phases = new();

    public void Regiseter<TPhase>(string sceneName) where TPhase : SceneGamePhase
    {
        phases[sceneName] = typeof(TPhase);
    }

    public Type Get(string sceneName)
    {
        return phases.TryGetValue(sceneName, out Type phase) ? phase : null;
    }
}
