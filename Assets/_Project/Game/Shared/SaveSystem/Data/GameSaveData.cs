using System;
using System.Collections.Generic;
using System.Linq;

namespace ChainSawLeg.Core.SaveSystem
{
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentFormatVersion = 1;

        private GameSaveData()
        {
        }

        public GameSaveData(
            GameSaveMetadata metadata,
            IEnumerable<GameSaveEntry> entries = null)
        {
            Metadata = metadata;
            Entries = entries?.ToArray() ?? Array.Empty<GameSaveEntry>();
        }

        public GameSaveMetadata Metadata { get; private set; }
        public IReadOnlyList<GameSaveEntry> Entries { get; private set; }

        public GameSaveData WithMetadata(GameSaveMetadata metadata)
        {
            return new GameSaveData(metadata, Entries);
        }
    }
}
