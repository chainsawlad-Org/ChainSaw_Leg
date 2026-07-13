using ChainSawLeg.Core.SaveSystem;

namespace ChainSawLeg.Features.Exploration.Save
{
    public static class ExplorationSceneIds
    {
        public const string World = "world";

        public static void Validate(string sceneId)
        {
            if (string.IsNullOrWhiteSpace(sceneId))
                throw new GameSaveValidationException("Exploration scene ID is required.");
        }
    }
}
