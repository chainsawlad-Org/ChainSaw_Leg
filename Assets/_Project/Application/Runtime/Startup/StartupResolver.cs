using System;

public class StartupResolver : IStartupResolver
{
    private readonly StartupPhaseRegistry registry;
    private readonly IActiveSceneProvider activeSceneProvider;

    public StartupResolver(
        StartupPhaseRegistry registry,
        IActiveSceneProvider activeSceneProvider)
    {
        this.registry = registry;
        this.activeSceneProvider = activeSceneProvider;
    }

    public Type Resolve()
    {
#if UNITY_EDITOR

        string activeScene = activeSceneProvider.ActiveSceneName;

        if (registry.TryGet(activeScene, out Type phase))
            return phase;

#endif

        return typeof(MainMenuPhase);
    }
}
