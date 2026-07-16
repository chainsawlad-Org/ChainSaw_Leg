using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionDetector : MonoBehaviour
{
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ExplorationInteractionConfig interactionConfig;

    private readonly Collider2D[] overlapResults = new Collider2D[64];
    private readonly List<InteractionTargetCandidate> candidates = new List<InteractionTargetCandidate>(16);
    private readonly List<DetectedInteractionTarget> detectedTargets = new List<DetectedInteractionTarget>(16);

    private ContactFilter2D contactFilter;
    private float scanTimer;
    private IInteractable currentTarget;
    private Collider2D currentTargetCollider;

    public event Action<IInteractable> CurrentTargetChanged;

    public IInteractable CurrentTarget => currentTarget;
    public Collider2D CurrentTargetCollider => currentTargetCollider;
    public bool HasTarget => currentTarget != null;
    public Transform InteractionOrigin => interactionOrigin;
    public Vector2 InteractionOriginPosition => GetInteractionOriginPosition();
    public ExplorationInteractionConfig InteractionConfig => interactionConfig;

    private void Awake()
    {
        if (interactionOrigin == null)
        {
            interactionOrigin = transform;
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }

        contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = interactionConfig != null
            ? interactionConfig.InteractionLayerMask
            : (LayerMask)Physics2D.AllLayers;
        contactFilter.useTriggers = true;
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (interactionConfig == null || playerMovement == null)
        {
            return;
        }

        scanTimer -= deltaTime;

        if (scanTimer > 0f)
        {
            return;
        }

        scanTimer = interactionConfig.ScanIntervalSeconds;
        ScanForTargets();
    }

    public void ClearTarget()
    {
        ApplyBestTarget(null, null);
    }

    private void ScanForTargets()
    {
        candidates.Clear();
        detectedTargets.Clear();

        contactFilter.layerMask = interactionConfig.InteractionLayerMask;
        Vector2 origin = GetInteractionOriginPosition();

        int overlapCount = Physics2D.OverlapCircle(
            origin,
            interactionConfig.InteractionRadius,
            contactFilter,
            overlapResults);

        Direction8 facingDirection = Direction8Utility.FromVector(playerMovement.LastMoveDir);
        Vector2 facingVector = Direction8Utility.ToVector(facingDirection);
        BuildCandidates(overlapCount, origin, facingVector);

        var selectionRules = new InteractionTargetSelectionRules(
            interactionConfig.DirectPriorityHalfAngleDegrees,
            interactionConfig.InteractionHalfAngleDegrees);

        if (InteractionTargetSelectionPolicy.TrySelectBestCandidate(
                candidates,
                selectionRules,
                out InteractionTargetCandidate bestCandidate))
        {
            DetectedInteractionTarget bestTarget = detectedTargets[bestCandidate.SourceIndex];
            ApplyBestTarget(bestTarget.Interactable, bestTarget.SourceCollider);
            return;
        }

        ClearTarget();
    }

    private void BuildCandidates(int overlapCount, Vector2 origin, Vector2 facingVector)
    {
        for (int index = 0; index < overlapCount; index++)
        {
            Collider2D sourceCollider = overlapResults[index];

            if (sourceCollider == null)
            {
                continue;
            }

            if (!TryBuildCandidate(
                    sourceCollider,
                    origin,
                    facingVector,
                    detectedTargets.Count,
                    out InteractionTargetCandidate candidate,
                    out DetectedInteractionTarget detectedTarget))
            {
                continue;
            }

            candidates.Add(candidate);
            detectedTargets.Add(detectedTarget);
        }
    }

    private bool TryBuildCandidate(
        Collider2D sourceCollider,
        Vector2 origin,
        Vector2 facingVector,
        int sourceIndex,
        out InteractionTargetCandidate candidate,
        out DetectedInteractionTarget detectedTarget)
    {
        candidate = null;
        detectedTarget = default;

        IInteractable interactable = ResolveInteractable(sourceCollider);

        if (interactable == null || !interactable.CanInteract())
        {
            return false;
        }

        Vector2 targetPoint = sourceCollider.ClosestPoint(origin);
        Vector2 toTarget = targetPoint - origin;

        float sqrDistance = toTarget.sqrMagnitude;

        float interactionRadius = interactionConfig.InteractionRadius;

        if (sqrDistance > interactionRadius * interactionRadius)
        {
            return false;
        }

        Vector2 directionToTarget = toTarget;

        if (directionToTarget.sqrMagnitude <= 0.0001f)
        {
            directionToTarget = (Vector2)sourceCollider.bounds.center - origin;
        }

        if (directionToTarget.sqrMagnitude <= 0.0001f)
        {
            directionToTarget = playerMovement.LastMoveDir;
        }

        float facingDot = Vector2.Dot(facingVector, directionToTarget.normalized);

        candidate = new InteractionTargetCandidate(
            sourceIndex,
            sqrDistance,
            facingDot,
            sourceCollider.GetInstanceID());
        detectedTarget = new DetectedInteractionTarget(interactable, sourceCollider);

        return true;
    }

    private void ApplyBestTarget(IInteractable nextTarget, Collider2D nextCollider)
    {
        if (ReferenceEquals(currentTarget, nextTarget) && currentTargetCollider == nextCollider)
        {
            return;
        }

        currentTarget = nextTarget;
        currentTargetCollider = nextCollider;

        CurrentTargetChanged?.Invoke(currentTarget);
    }

    private static IInteractable ResolveInteractable(Collider2D sourceCollider)
    {
        MonoBehaviour[] behaviours = sourceCollider.GetComponentsInParent<MonoBehaviour>(true);

        for (int index = 0; index < behaviours.Length; index++)
        {
            if (behaviours[index] is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private Vector2 GetInteractionOriginPosition()
    {
        if (interactionOrigin != null && playerRigidbody != null)
        {
            if (interactionOrigin.parent == playerRigidbody.transform)
            {
                return playerRigidbody.GetRelativePoint(interactionOrigin.localPosition);
            }

            Vector2 localOrigin = playerRigidbody.transform.InverseTransformPoint(
                interactionOrigin.position);

            return playerRigidbody.GetRelativePoint(localOrigin);
        }

        if (interactionOrigin != null)
        {
            return interactionOrigin.position;
        }

        return playerRigidbody != null ? playerRigidbody.position : (Vector2)transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionConfig == null)
        {
            return;
        }

        Vector3 origin = GetInteractionOriginPosition();
        Direction8 facingDirection = playerMovement != null
            ? Direction8Utility.FromVector(playerMovement.LastMoveDir)
            : Direction8.Up;
        Vector2 facingVector = Direction8Utility.ToVector(facingDirection);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, interactionConfig.InteractionRadius);

        Gizmos.color = Color.cyan;
        float halfAngle = interactionConfig.InteractionHalfAngleDegrees;
        Gizmos.DrawRay(origin, Rotate(facingVector, -halfAngle) * interactionConfig.InteractionRadius);
        Gizmos.DrawRay(origin, Rotate(facingVector, halfAngle) * interactionConfig.InteractionRadius);
    }

    private static Vector2 Rotate(Vector2 direction, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cosine = Mathf.Cos(angleRadians);
        float sine = Mathf.Sin(angleRadians);

        return new Vector2(
            direction.x * cosine - direction.y * sine,
            direction.x * sine + direction.y * cosine);
    }

    private readonly struct DetectedInteractionTarget
    {
        public DetectedInteractionTarget(IInteractable interactable, Collider2D sourceCollider)
        {
            Interactable = interactable;
            SourceCollider = sourceCollider;
        }

        public IInteractable Interactable { get; }
        public Collider2D SourceCollider { get; }
    }
}
