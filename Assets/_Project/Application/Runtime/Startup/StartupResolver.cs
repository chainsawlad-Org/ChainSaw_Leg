using System;
using ChainSawLeg.Infrastructure;
using UnityEditor;

namespace ChainSawLeg.Application
{
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

            string startupScene = SessionState.GetString(
                "ChainSawLeg.StartupScene",
                string.Empty);

            if (!string.IsNullOrEmpty(startupScene) &&
                registry.TryGet(startupScene, out Type startupPhase))
            {
                return startupPhase;
            }

            string activeScene = activeSceneProvider.ActiveSceneName;

            if (registry.TryGet(activeScene, out Type activePhase))
                return activePhase;
#endif

            return typeof(MainMenuPhase);
        }
    }
}
