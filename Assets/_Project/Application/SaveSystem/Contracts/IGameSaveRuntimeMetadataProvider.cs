namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveRuntimeMetadataProvider
    {
        string ProfileId { get; }
        string BuildNumber { get; }
    }
}
