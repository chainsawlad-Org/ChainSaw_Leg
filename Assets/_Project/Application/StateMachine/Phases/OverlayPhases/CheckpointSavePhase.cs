public class CheckpointSavePhase : OverlayPhase
{
    public override bool BlocksInput => true;
    public override bool PausesGame => true;
    public override bool CanStack => false;
}
