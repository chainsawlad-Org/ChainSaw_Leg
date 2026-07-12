using System;
using ChainSawLeg.Core.SaveSystem;

namespace ChainSawLeg.Features.Exploration.Save
{
    public static class ExplorationSceneIds
    {
        public const string World = "world";

        public static string ResolveSceneName(string sceneId)
        {
            if (string.IsNullOrWhiteSpace(sceneId))
                throw new GameSaveValidationException("Exploration scene ID is required.");

            if (sceneId == World)
                return SceneNames.World;

            if (!sceneId.StartsWith("SC_", StringComparison.Ordinal) ||
                sceneId == SceneNames.Persistent ||
                sceneId == SceneNames.MainMenu ||
                sceneId == SceneNames.Battle)
                throw new GameSaveValidationException($"Unknown exploration scene ID: {sceneId}.");

            return sceneId;
        }
    }
}
