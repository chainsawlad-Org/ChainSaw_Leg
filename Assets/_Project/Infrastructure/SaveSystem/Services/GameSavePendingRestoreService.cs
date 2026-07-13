namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class GameSavePendingRestoreService
    {
        private GameSaveData pendingSaveData;

        public bool HasPendingRestore => pendingSaveData != null;

        public void SetPending(GameSaveData saveData)
        {
            pendingSaveData = saveData ?? throw new GameSaveValidationException("Pending game save data is required.");
        }

        public GameSaveData GetPending()
        {
            if (pendingSaveData == null)
                throw new GameSaveValidationException("Pending game save data is not available.");

            return pendingSaveData;
        }

        public void Clear()
        {
            pendingSaveData = null;
        }
    }
}
