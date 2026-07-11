using System;

public class GameplayInputBlockService
{
    private int moveBlockCount;
    private int dashBlockCount;
    private int interactBlockCount;
    private int submitBlockCount;

    public event Action BlockStateChanged;

    public bool IsBlocked => IsChannelBlocked(InputBlockChannels.Gameplay);
    public int ActiveBlockCount => moveBlockCount + dashBlockCount + interactBlockCount + submitBlockCount;

    public bool IsChannelBlocked(InputBlockChannels channels)
    {
        if ((channels & InputBlockChannels.Move) != 0 && moveBlockCount > 0)
            return true;

        if ((channels & InputBlockChannels.Dash) != 0 && dashBlockCount > 0)
            return true;

        if ((channels & InputBlockChannels.Interact) != 0 && interactBlockCount > 0)
            return true;

        if ((channels & InputBlockChannels.Submit) != 0 && submitBlockCount > 0)
            return true;

        return false;
    }

    public void AcquireBlock(InputBlockChannels channels)
    {
        if (channels == InputBlockChannels.None)
            return;

        ModifyCounts(channels, 1);
        BlockStateChanged?.Invoke();
    }

    public void ReleaseBlock(InputBlockChannels channels)
    {
        if (channels == InputBlockChannels.None)
            return;

        ModifyCounts(channels, -1);
        BlockStateChanged?.Invoke();
    }

    public void Reset()
    {
        if (ActiveBlockCount == 0)
            return;

        moveBlockCount = 0;
        dashBlockCount = 0;
        interactBlockCount = 0;
        submitBlockCount = 0;
        BlockStateChanged?.Invoke();
    }

    private void ModifyCounts(InputBlockChannels channels, int delta)
    {
        if ((channels & InputBlockChannels.Move) != 0)
            moveBlockCount = Math.Max(0, moveBlockCount + delta);

        if ((channels & InputBlockChannels.Dash) != 0)
            dashBlockCount = Math.Max(0, dashBlockCount + delta);

        if ((channels & InputBlockChannels.Interact) != 0)
            interactBlockCount = Math.Max(0, interactBlockCount + delta);

        if ((channels & InputBlockChannels.Submit) != 0)
            submitBlockCount = Math.Max(0, submitBlockCount + delta);
    }
}
