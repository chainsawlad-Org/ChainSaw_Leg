using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionDetector : MonoBehaviour
{
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ExplorationInteractionConfig interactionConfig;

    private readonly Collider2D[] overlapResults = new Collider2D[32];
    private readonly List<InteractionTargetCandidate> candidates = new List<InteractionTargetCandidate>(16);
    private readonly HashSet<IInteractable> uniqueInteractables = new HashSet<IInteractable>();
    private readonly InteractionTargetSelectionPolicy targetSelectionPolicy = new InteractionTargetSelectionPolicy();

    private ContactFilter2D contactFilter;
    private float scanTimer;
    private IInteractable currentTarget;
    private Collider2D currentTargetCollider;

    public event Action<IInteractable> CurrentTargetChanged;

    public IInteractable CurrentTarget => currentTarget;
    public Collider2D CurrentTargetCollider => currentTargetCollider;
    public bool HasTarget => currentTarget != null;
    public Transform InteractionOrigin => interactionOrigin;
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

        contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = interactionConfig != null ? interactionConfig.InteractionLayerMask : Physics2D.AllLayers;
        contactFilter.useTriggers = true;
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (interactionConfig == null || interactionOrigin == null || playerMovement == null)
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
        uniqueInteractables.Clear();

        contactFilter.layerMask = interactionConfig.InteractionLayerMask;

        int overlapCount = Physics2D.OverlapCircle(
            interactionOrigin.position,
            interactionConfig.InteractionRadius,
            contactFilter,
            overlapResults);

        BuildCandidates(overlapCount);

        if (targetSelectionPolicy.TrySelectBestCandidate(candidates, currentTarget, out InteractionTargetCandidate bestCandidate))
        {
            ApplyBestTarget(bestCandidate.Interactable, bestCandidate.SourceCollider);
            return;
        }

        ClearTarget();
    }

    private void BuildCandidates(int overlapCount)
    {
        for (int index = 0; index < overlapCount; index++)
        {
            Collider2D sourceCollider = overlapResults[index];

            if (sourceCollider == null)
            {
                continue;
            }

            if (!TryBuildCandidate(sourceCollider, out InteractionTargetCandidate candidate))
            {
                continue;
            }

            if (!uniqueInteractables.Add(candidate.Interactable))
            {
                continue;
            }

            candidates.Add(candidate);
        }
    }

    private bool TryBuildCandidate(Collider2D sourceCollider, out InteractionTargetCandidate candidate)
    {
        candidate = null;

        IInteractable interactable = ResolveInteractable(sourceCollider);

        if (interactable == null || !interactable.CanInteract())
        {
            return false;
        }

        Vector2 origin = interactionOrigin.position;
        Vector2 targetPoint = sourceCollider.bounds.center;
        Vector2 toTarget = targetPoint - origin;

        float sqrDistance = toTarget.sqrMagnitude;

        if (sqrDistance <= 0.0001f)
        {
            return false;
        }

        Vector2 facingDirection = playerMovement.LastMoveDir;

        if (facingDirection.sqrMagnitude <= 0.0001f)
        {
            facingDirection = Vector2.up;
        }

        facingDirection.Normalize();

        Vector2 directionToTarget = toTarget.normalized;
        float facingDot = Vector2.Dot(facingDirection, directionToTarget);

        if (facingDot < interactionConfig.FacingDotThreshold)
        {
            return false;
        }

        candidate = new InteractionTargetCandidate(
            interactable,
            sourceCollider,
            sqrDistance,
            facingDot,
            targetPoint);

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
}