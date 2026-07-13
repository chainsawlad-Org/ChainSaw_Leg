namespace ChainSawLeg.Features.Exploration.Save
{
    public interface IPlayerPositionProvider
    {
        bool IsPlayerAvailable { get; }
        float PositionX { get; }
        float PositionY { get; }
    }
}
