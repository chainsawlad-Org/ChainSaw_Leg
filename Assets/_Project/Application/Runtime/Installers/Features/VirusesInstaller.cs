using ChainSawLeg.Composition;
using ChainSawLeg.Features.Minigames;
using UnityEngine;
using Zenject;

public class VirusesInstaller : MonoInstaller
{
    [SerializeField] private VirusesGameManager gameManager;
    [SerializeField] private LevelTwoController levelTwoController;

    [SerializeField] private CursorContoll cursorContoll;
    [SerializeField] private GameObject cursor;
    [SerializeField] private GameObject mainCamera;
    public override void InstallBindings()
    {
        Container.Bind<VirusesGameManager>()
                .FromInstance(gameManager)
                .AsSingle();

        Container.Bind<LevelTwoController>()
                .FromInstance(levelTwoController)
                .AsSingle();

        Container.Bind<CursorContoll>()
                .FromInstance(cursorContoll)
                .AsSingle();

        Container.Bind<GameObject>()
                .WithId("Cursor")
                .FromInstance(cursor)
                .AsCached();

        Container.Bind<GameObject>()
                .WithId("MainCamera")
                .FromInstance(mainCamera)
                .AsCached();

        Container.BindInterfacesTo<VirusesComposition>()
                .AsSingle()
                .NonLazy();
    }
}