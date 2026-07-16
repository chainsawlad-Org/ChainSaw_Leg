using System;
using System.Collections.Generic;

public readonly struct InteractionTargetSelectionRules
{
    private const double DegreesToRadians = Math.PI / 180d;

    public InteractionTargetSelectionRules(
        float directPriorityHalfAngleDegrees,
        float maximumAcceptedHalfAngleDegrees)
    {
        if (directPriorityHalfAngleDegrees < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(directPriorityHalfAngleDegrees));
        }

        if (maximumAcceptedHalfAngleDegrees < directPriorityHalfAngleDegrees ||
            maximumAcceptedHalfAngleDegrees > 180f)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAcceptedHalfAngleDegrees));
        }

        DirectPriorityDotThreshold = CalculateDotThreshold(directPriorityHalfAngleDegrees);
        MaximumAcceptedDotThreshold = CalculateDotThreshold(maximumAcceptedHalfAngleDegrees);
    }

    public float DirectPriorityDotThreshold { get; }
    public float MaximumAcceptedDotThreshold { get; }

    private static float CalculateDotThreshold(float halfAngleDegrees)
    {
        return (float)Math.Cos(halfAngleDegrees * DegreesToRadians);
    }
}

public static class InteractionTargetSelectionPolicy
{
    private const float FloatingPointTolerance = 0.000001f;

    public static bool TrySelectBestCandidate(
        IReadOnlyList<InteractionTargetCandidate> candidates,
        InteractionTargetSelectionRules rules,
        out InteractionTargetCandidate bestCandidate)
    {
        bestCandidate = null;

        if (candidates == null || candidates.Count == 0)
        {
            return false;
        }

        for (int index = 0; index < candidates.Count; index++)
        {
            InteractionTargetCandidate candidate = candidates[index];

            if (candidate == null)
            {
                continue;
            }

            int directionPriority = GetDirectionPriority(candidate, rules);

            if (directionPriority < 0)
            {
                continue;
            }

            if (bestCandidate == null || IsPreferred(candidate, directionPriority, bestCandidate, rules))
            {
                bestCandidate = candidate;
            }
        }

        return bestCandidate != null;
    }

    private static bool IsPreferred(
        InteractionTargetCandidate candidate,
        int candidateDirectionPriority,
        InteractionTargetCandidate currentBest,
        InteractionTargetSelectionRules rules)
    {
        int currentBestDirectionPriority = GetDirectionPriority(currentBest, rules);

        if (candidateDirectionPriority != currentBestDirectionPriority)
        {
            return candidateDirectionPriority < currentBestDirectionPriority;
        }

        int distanceComparison = candidate.SqrDistance.CompareTo(currentBest.SqrDistance);

        if (distanceComparison != 0)
        {
            return distanceComparison < 0;
        }

        return candidate.StableId < currentBest.StableId;
    }

    private static int GetDirectionPriority(
        InteractionTargetCandidate candidate,
        InteractionTargetSelectionRules rules)
    {
        if (candidate.FacingDot + FloatingPointTolerance >= rules.DirectPriorityDotThreshold)
        {
            return 0;
        }

        if (candidate.FacingDot + FloatingPointTolerance >= rules.MaximumAcceptedDotThreshold)
        {
            return 1;
        }

        return -1;
    }
}
