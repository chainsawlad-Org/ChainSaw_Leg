public class SaveBrowserPhase : OverlayPhase
{
    public override bool BlocksInput => true;
    public override bool PausesGame => true;
    public override bool CanStack => false;
}
