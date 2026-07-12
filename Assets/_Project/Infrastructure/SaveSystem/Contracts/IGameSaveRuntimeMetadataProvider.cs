// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveRuntimeMetadataProvider
    {
        string ProfileId { get; }
        string BuildNumber { get; }
    }
}
