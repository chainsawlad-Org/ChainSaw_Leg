using System.Collections.Generic;

public class InteractionTargetSelectionPolicy
{
    public bool TrySelectBestCandidate(
        IReadOnlyList<InteractionTargetCandidate> candidates,
        IInteractable currentTarget,
        out InteractionTargetCandidate bestCandidate)
    {
        bestCandidate = null;

        if (candidates == null || candidates.Count == 0)
        {
            return false;
        }

        float bestScore = float.MinValue;

        for (int index = 0; index < candidates.Count; index++)
        {
            InteractionTargetCandidate candidate = candidates[index];

            float score = CalculateScore(candidate, currentTarget);

            if (score > bestScore)
            {
                bestScore = score;
                bestCandidate = candidate;
            }
        }

        return bestCandidate != null;
    }

    private float CalculateScore(InteractionTargetCandidate candidate, IInteractable currentTarget)
    {
        float distanceScore = -candidate.SqrDistance;
        float facingScore = candidate.FacingDot * 2f;
        float currentTargetBias = ReferenceEquals(candidate.Interactable, currentTarget) ? 0.25f : 0f;

        return distanceScore + facingScore + currentTargetBias;
    }
}