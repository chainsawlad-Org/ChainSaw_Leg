// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveMigrationStep
    {
        int SourceVersion { get; }
        int TargetVersion { get; }
        GameSaveData Migrate(GameSaveData saveData);
    }
}
