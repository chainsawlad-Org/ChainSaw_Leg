using ChainSawLeg.Application;
using ChainSawLeg.Infrastructure;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class PhaseInstaller : Installer<PhaseInstaller>
    {
        public override void InstallBindings()
        {
            AutoBinder.BindDerivedTypes<GamePhase>(Container);
        }
    }
}
