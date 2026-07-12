// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

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
