using Zenject;

public sealed class DialogueInstaller : Installer<DialogueInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<DialogueRuntimeRegistry>().AsSingle();
        Container.Bind<DialogueService>().AsSingle();
        Container.Bind<DialoguePhase>().AsSingle();
    }
}
