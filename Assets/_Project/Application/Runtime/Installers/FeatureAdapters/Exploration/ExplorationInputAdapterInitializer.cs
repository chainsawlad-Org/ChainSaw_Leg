using Zenject;

public sealed class ExplorationInputAdapterInitializer : IInitializable
{
    private readonly PlayerInputHandler inputHandler;
    private readonly IGameInputService inputService;

    public ExplorationInputAdapterInitializer(
        PlayerInputHandler inputHandler,
        IGameInputService inputService)
    {
        this.inputHandler = inputHandler;
        this.inputService = inputService;
    }

    public void Initialize()
    {
        inputHandler.Initialize(inputService);
    }
}
