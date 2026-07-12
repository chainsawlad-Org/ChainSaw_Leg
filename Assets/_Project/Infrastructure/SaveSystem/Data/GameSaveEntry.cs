// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveEntry
    {
        public string ContributorId;
        public byte[] Payload;
    }
}
