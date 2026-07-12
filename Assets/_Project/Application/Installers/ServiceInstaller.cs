using Zenject;

public class ServiceInstaller : Installer<ServiceInstaller>
{
    public override void InstallBindings()
    {
        AutoBinder.BindDerivedTypes<SceneService>(Container);
    }
}