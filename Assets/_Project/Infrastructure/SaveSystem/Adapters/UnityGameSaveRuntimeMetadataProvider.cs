// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using UnityEngine;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class UnityGameSaveRuntimeMetadataProvider : IGameSaveRuntimeMetadataProvider
    {
        public string ProfileId => "default";
        public string BuildNumber => string.IsNullOrWhiteSpace(Application.version)
            ? "development"
            : Application.version;
    }
}
