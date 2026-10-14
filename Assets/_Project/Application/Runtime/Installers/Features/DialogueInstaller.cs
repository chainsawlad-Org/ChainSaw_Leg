using ChainSawLeg.Features.Dialogue;
using Zenject;

namespace ChainSawLeg.Composition
{
    public sealed class DialogueInstaller : Installer<DialogueInstaller>
    {
        public override void InstallBindings()
        {
            // Container.BindInterfacesAndSelfTo<DialogueRuntimeRegistry>().AsSingle();
            // Container.Bind<DialogueService>().AsSingle();
            Container.Bind<DialogueJsonReader>()
                .AsSingle();

            Container.Bind<DialogueValidator>()
                .AsSingle();

            Container.Bind<DialogueBuilder>()
                .AsSingle();

            Container.Bind<DialogueImporter>()
                .AsSingle();
            
            // Container.Bind<IDialogueDatabase>()
            //     .To<DialogueDatabaseService>()
            //     .FromResolve();
            
            Container.BindInterfacesAndSelfTo<DialogueDatabaseService>()
                .AsSingle();
        }
    }
}
