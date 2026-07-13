using Zenject;

public sealed class DialogueInstaller : Installer<DialogueInstaller>
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<DialogueRuntimeRegistry>().AsSingle();
        Container.Bind<DialogueService>().AsSingle();
    }
}
