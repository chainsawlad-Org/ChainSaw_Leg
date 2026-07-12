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
        registry.Regiseter<MainMenuPhase>(SceneNames.MainMenu);
        registry.Regiseter<ExplorationPhase>(SceneNames.World);
    }
}
