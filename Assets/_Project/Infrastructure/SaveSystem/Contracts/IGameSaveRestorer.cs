// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

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
