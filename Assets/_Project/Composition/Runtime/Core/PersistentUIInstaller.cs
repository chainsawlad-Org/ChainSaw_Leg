using ChainSawLeg.Application;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Dialogue;
using ChainSawLeg.UI;
using UnityEngine;
using Zenject;

namespace ChainSawLeg.Composition
{
    public class PersistentUIInstaller : MonoInstaller
    {
        [SerializeField] private Transform uiRoot;
        [SerializeField] private DialogueUI_RPG dialogueUI;

        public override void InstallBindings()
        {
            Container.Bind<PersistentUIViewRegistry>()
                .AsSingle()
                .WithArguments(uiRoot, GameSaveSlotCatalog.CheckpointSlotIds.Count)
                .NonLazy();

            Container.Bind<PauseMenuView>()
                .FromResolveGetter<PersistentUIViewRegistry>(registry => registry.PauseMenuView)
                .AsSingle();

            Container.Bind<SaveBrowserView>()
                .FromResolveGetter<PersistentUIViewRegistry>(registry => registry.SaveBrowserView)
                .AsSingle();

            Container.BindInterfacesTo<PauseMenuCoordinator>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<MainMenuSaveBrowserCoordinator>()
                .AsSingle()
                .NonLazy();

            Container.Bind<CheckpointSaveMenuView>()
                .FromResolveGetter<PersistentUIViewRegistry>(registry => registry.CheckpointSaveMenuView)
                .AsSingle();

            Container.BindInterfacesTo<CheckpointSaveMenuCoordinator>()
                .AsSingle()
                .NonLazy();
            
            if (dialogueUI == null)
            {
                throw new System.InvalidOperationException(
                    "Persistent dialogue UI is not assigned.");
            }

            Container.Bind<DialogueUI_RPG>()
                .FromInstance(dialogueUI)
                .AsSingle();
            
            Container.Resolve<DialogueViewRegistry>()
                .Register(dialogueUI);
        }
    }
}
