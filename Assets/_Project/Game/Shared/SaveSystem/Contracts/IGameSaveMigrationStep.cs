namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveMigrationStep
    {
        int SourceVersion { get; }
        int TargetVersion { get; }
        GameSaveData Migrate(GameSaveData saveData);
    }
}
