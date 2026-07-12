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
