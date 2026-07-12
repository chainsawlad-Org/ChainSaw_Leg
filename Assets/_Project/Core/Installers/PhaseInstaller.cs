using Zenject;

public class PhaseInstaller : Installer<PhaseInstaller>
{
    public override void InstallBindings()
    {
        AutoBinder.BindDerivedTypes<GamePhase>(Container);
        Container.Bind<DialoguePhase>().AsSingle();
    }
}
