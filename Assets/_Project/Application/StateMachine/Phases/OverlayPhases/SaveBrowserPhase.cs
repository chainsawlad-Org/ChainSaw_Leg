// Placement: Docs/Ru/02_ProjectStructure.md:98-116. Quote: "StateMachine знает только о жизненном цикле фаз."

public class SaveBrowserPhase : OverlayPhase
{
    public override bool BlocksInput => true;
    public override bool PausesGame => true;
    public override bool CanStack => false;
}
