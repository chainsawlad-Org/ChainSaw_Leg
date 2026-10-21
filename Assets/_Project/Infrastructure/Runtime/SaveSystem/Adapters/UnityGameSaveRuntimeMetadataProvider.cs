using ChainSawLeg.Core.SaveSystem;
using UnityEngine;

namespace ChainSawLeg.Infrastructure
{
    public sealed class UnityGameSaveRuntimeMetadataProvider : IGameSaveRuntimeMetadataProvider
    {
        public string ProfileId => "default";
        public string BuildNumber => string.IsNullOrWhiteSpace(Application.version)
            ? "development"
            : Application.version;
    }
}
