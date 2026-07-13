using Zenject;

public class BattleInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<CombatEventBus>().AsSingle();
        Container.Bind<PlayerActionController>().AsSingle();
        Container.Bind<CombatResolver>().AsSingle();
        Container.Bind<SimpleAI>().AsSingle();
        Container.Bind<BattleSessionController>().AsSingle();
    }
}
