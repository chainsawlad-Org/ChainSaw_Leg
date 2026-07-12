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
