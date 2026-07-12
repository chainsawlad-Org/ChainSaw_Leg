// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;
using System.Collections.Generic;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentFormatVersion = 1;

        public GameSaveMetadata Metadata;
        public List<GameSaveEntry> Entries = new();
    }
}
