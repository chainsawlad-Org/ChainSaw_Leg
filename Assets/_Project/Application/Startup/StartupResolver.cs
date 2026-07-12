using System;
using UnityEngine.SceneManagement;

public class StartupResolver : IStartupResolver
{
    private readonly StartupPhaseRegistry registry;

    public StartupResolver(StartupPhaseRegistry registry)
    {
        this.registry = registry;
    }

    public Type Resolve()
    {
#if UNITY_EDITOR

        string activeScene = SceneManager.GetActiveScene().name;

        if (registry.TryGet(activeScene, out Type phase))
            return phase;

#endif

        return typeof(MainMenuPhase);
    }
}