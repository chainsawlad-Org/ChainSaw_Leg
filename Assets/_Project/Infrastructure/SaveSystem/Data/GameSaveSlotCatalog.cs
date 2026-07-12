// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System;
using System.Collections.Generic;

namespace ChainSawLeg.Core.SaveSystem
{
    public static class GameSaveSlotCatalog
    {
        private static readonly string[] checkpointSlotIds =
        {
            "checkpoint_0",
            "checkpoint_1",
            "checkpoint_2",
            "checkpoint_3",
            "checkpoint_4",
            "checkpoint_5",
            "checkpoint_6",
            "checkpoint_7",
            "checkpoint_8",
            "checkpoint_9"
        };

        public static IReadOnlyList<string> CheckpointSlotIds => checkpointSlotIds;

        public static string CreateAutoSlotId(int index)
        {
            return CreateIndexedSlotId("auto", index);
        }

        public static string CreateManualSlotId(int index)
        {
            return CreateIndexedSlotId("manual", index);
        }

        private static string CreateIndexedSlotId(string prefix, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            return $"{prefix}_{index}";
        }
    }
}
