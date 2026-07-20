using System;

public interface IGameplayInputBlockService
{
    event Action BlockStateChanged;

    bool IsBlocked { get; }
    int ActiveBlockCount { get; }

    bool IsChannelBlocked(InputBlockChannels channels);
    void AcquireBlock(InputBlockChannels channels);
    void ReleaseBlock(InputBlockChannels channels);
    void Reset();
}
