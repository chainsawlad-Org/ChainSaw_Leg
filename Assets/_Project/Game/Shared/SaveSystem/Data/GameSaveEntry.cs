using System;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveEntry
    {
        private GameSaveEntry()
        {
        }

        public GameSaveEntry(string contributorId, byte[] payload)
        {
            ContributorId = contributorId;
            Payload = payload;
        }

        public string ContributorId { get; private set; }
        public byte[] Payload { get; private set; }
    }
}
