using ChainSawLeg.Composition;
using ChainSawLeg.Features.Minigames;
using UnityEngine;
using Zenject;

public class VirusesInstaller : MonoInstaller
{
    [SerializeField]
    private VirusesGameManager gameManager;
    [SerializeField]
    private GameObject cursor;
    public override void InstallBindings()
    {
        Container.Bind<VirusesGameManager>()
                .FromInstance(gameManager)
                .AsSingle();

        Container.Bind<GameObject>()
                .FromInstance(cursor)
                .AsSingle();

        Container.BindInterfacesTo<VirusesComposition>()
                .AsSingle()
                .NonLazy();
    }
}