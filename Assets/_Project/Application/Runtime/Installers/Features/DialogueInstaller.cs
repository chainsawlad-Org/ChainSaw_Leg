using ChainSawLeg.Features.Dialogue;
using Zenject;

namespace ChainSawLeg.Composition
{
    public sealed class DialogueInstaller : Installer<DialogueInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<DialogueJsonReader>()
                .AsSingle();

            Container.Bind<DialogueValidator>()
                .AsSingle();

            Container.Bind<DialogueBuilder>()
                .AsSingle();

            Container.Bind<DialogueImporter>()
                .AsSingle();
            
            Container.BindInterfacesAndSelfTo<DialogueDatabaseService>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<DialogueRunner>().AsSingle();
        }
    }
}
