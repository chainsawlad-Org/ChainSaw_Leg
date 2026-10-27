using ChainSawLeg.Features.Minigames;
using UnityEngine;
using Zenject;

public class VirusesInstaller : MonoInstaller
{
    [SerializeField]
    public VirusesGameManager gameManager;
    public override void InstallBindings()
    {
        Container.Bind<VirusesGameManager>()
                .FromInstance(gameManager)
                .AsSingle();

        Container.BindInterfacesTo<LevelTwoController>()
                .AsSingle()
                .NonLazy();
    }
}