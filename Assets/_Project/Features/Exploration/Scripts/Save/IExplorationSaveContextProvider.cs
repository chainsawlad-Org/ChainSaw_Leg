namespace ChainSawLeg.Features.Exploration.Save
{
    public interface IExplorationSaveContextProvider
    {
        string SceneId { get; }
        string CheckpointId { get; }
    }
}
