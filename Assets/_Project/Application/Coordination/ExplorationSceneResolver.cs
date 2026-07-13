using System;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;

public static class ExplorationSceneResolver
{
    public static string ResolveSceneName(string sceneId)
    {
        ExplorationSceneIds.Validate(sceneId);

        if (sceneId == ExplorationSceneIds.World)
            return SceneNames.World;

        if (!sceneId.StartsWith("SC_", StringComparison.Ordinal) ||
            sceneId == SceneNames.Persistent ||
            sceneId == SceneNames.MainMenu ||
            sceneId == SceneNames.Battle)
            throw new GameSaveValidationException($"Unknown exploration scene ID: {sceneId}.");

        return sceneId;
    }
}
