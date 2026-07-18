using Zenject;

public class StartupRegistryInstaller : IInitializable
{
    private readonly StartupPhaseRegistry registry;

    public StartupRegistryInstaller(StartupPhaseRegistry startupPhaseRegistry)
    {
        this.registry = startupPhaseRegistry;
    }

    public void Initialize()
    {
        registry.Register<MainMenuPhase>(SceneNames.MainMenu);
        registry.Register<ExplorationPhase>(SceneNames.World);
        registry.Register<WorldOldExplorationPhase>(SceneNames.WorldOld);
        registry.Register<BattlePhase>(SceneNames.Battle);
    }
}
