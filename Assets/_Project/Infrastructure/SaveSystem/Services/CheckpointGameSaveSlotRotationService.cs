using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Core.SaveSystem
{
    public sealed class CheckpointGameSaveSlotRotationService
    {
        private readonly IGameSaveStorageProvider storageProvider;

        public CheckpointGameSaveSlotRotationService(IGameSaveStorageProvider storageProvider)
        {
            this.storageProvider = storageProvider;
        }

        public async UniTask<string> GetNextSlotIdAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<GameSaveSlotInfo> slots = await storageProvider.ListSlotsAsync(cancellationToken);
            Dictionary<string, GameSaveSlotInfo> checkpointSlots = slots
                .Where(slot => GameSaveSlotCatalog.CheckpointSlotIds.Contains(slot.SlotId))
                .ToDictionary(slot => slot.SlotId, StringComparer.Ordinal);

            foreach (string slotId in GameSaveSlotCatalog.CheckpointSlotIds)
            {
                if (!checkpointSlots.TryGetValue(slotId, out GameSaveSlotInfo slot) || slot.IsCorrupted)
                    return slotId;
            }

            return checkpointSlots.Values
                .OrderBy(slot => slot.Metadata.UtcTimestamp)
                .ThenBy(slot => slot.SlotId, StringComparer.Ordinal)
                .First()
                .SlotId;
        }
    }
}
