using System;

namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveContributor
    {
        string ContributorId { get; }
        Type SaveDataType { get; }
        object CaptureSaveData();
    }
}
