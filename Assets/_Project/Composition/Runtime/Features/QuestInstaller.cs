using ChainSawLeg.Features.Quest;
using Zenject;

namespace ChainSawLeg.Composition
{
    public sealed class QuestInstaller : Installer<QuestInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MissionProgressionService>()
                .AsSingle();
        }
    }
}
