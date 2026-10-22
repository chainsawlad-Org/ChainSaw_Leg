using System.Collections.Generic;
using Zenject;

public sealed class ExplorationInputAdapterInitializer : IInitializable
{
    private readonly List<PlayerInputHandler> inputHandlers;
    private readonly IGameInputService inputService;

    public ExplorationInputAdapterInitializer(
        List<PlayerInputHandler> inputHandlers,
        IGameInputService inputService)
    {
        this.inputHandlers = inputHandlers;
        this.inputService = inputService;
    }

    public void Initialize()
    {
        foreach (var handler in inputHandlers)
        {
            handler.Initialize(inputService);
        }
    }
}