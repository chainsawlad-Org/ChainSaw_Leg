using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BootstrapStartup : IInitializable
{
    private readonly IBootstrapRunner bootstrapRunner;

    public BootstrapStartup(
        IBootstrapRunner bootstrapRunner)
    {
        this.bootstrapRunner = bootstrapRunner;
    }

    public void Initialize()
    {
        Debug.Log("========== Bootstrap Initialize ==========");

        bootstrapRunner.Run().Forget();
    }


}