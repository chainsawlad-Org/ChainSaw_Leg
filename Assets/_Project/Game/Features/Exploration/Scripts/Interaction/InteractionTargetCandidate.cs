public class InteractionTargetCandidate
{
    public InteractionTargetCandidate(
        int sourceIndex,
        float sqrDistance,
        float facingDot,
        int stableId)
    {
        SourceIndex = sourceIndex;
        SqrDistance = sqrDistance;
        FacingDot = facingDot;
        StableId = stableId;
    }

    public int SourceIndex { get; }
    public float SqrDistance { get; }
    public float FacingDot { get; }
    public int StableId { get; }
}
