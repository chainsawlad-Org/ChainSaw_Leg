// Placement: Docs/Ru/01_Architecture.md:71-83. Quote: "- Save System".

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ChainSawLeg.Core.SaveSystem
{
    public interface IGameSaveStorageProvider
    {
        UniTask WriteAsync(GameSaveRequest request, byte[] data, CancellationToken cancellationToken);
        UniTask<byte[]> ReadAsync(GameSaveRequest request, CancellationToken cancellationToken);
        UniTask<IReadOnlyList<GameSaveSlotInfo>> ListSlotsAsync(CancellationToken cancellationToken);
        UniTask<bool> SlotExistsAsync(GameSaveRequest request, CancellationToken cancellationToken);
        UniTask DeleteSlotAsync(GameSaveRequest request, CancellationToken cancellationToken);
        UniTask<GameSaveMetadata> ReadMetadataAsync(
            GameSaveRequest request,
            CancellationToken cancellationToken);
    }
}
