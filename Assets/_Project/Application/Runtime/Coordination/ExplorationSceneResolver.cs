using System;
using ChainSawLeg.Core.SaveSystem;
using ChainSawLeg.Features.Exploration.Save;
using ChainSawLeg.Game.Shared;

public static class ExplorationSceneResolver
{
    public static string ResolveSceneName(string sceneId)
    {
        ExplorationSceneIds.Validate(sceneId);
        
        if (!sceneId.StartsWith("SC_", StringComparison.Ordinal) ||
            sceneId == SceneNames.Persistent ||
            sceneId == SceneNames.MainMenu)
            throw new GameSaveValidationException($"Unknown exploration scene ID: {sceneId}.");

        return sceneId;
    }
}
