using System;

namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveRestorer
    {
        string ContributorId { get; }
        Type SaveDataType { get; }
        void RestoreSaveData(object saveData);
    }
}
