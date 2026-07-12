using UnityEngine;

public class InteractionTargetCandidate
{
    public InteractionTargetCandidate(
        IInteractable interactable,
        Collider2D sourceCollider,
        float sqrDistance,
        float facingDot,
        Vector2 worldPoint)
    {
        Interactable = interactable;
        SourceCollider = sourceCollider;
        SqrDistance = sqrDistance;
        FacingDot = facingDot;
        WorldPoint = worldPoint;
    }

    public IInteractable Interactable { get; }
    public Collider2D SourceCollider { get; }
    public float SqrDistance { get; }
    public float FacingDot { get; }
    public Vector2 WorldPoint { get; }
}