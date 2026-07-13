namespace ChainSawLeg.Features.Exploration.Save
{
    public interface IPlayerPositionRestorationTarget
    {
        bool IsPlayerAvailable { get; }
        void RestorePosition(float positionX, float positionY);
    }
}
