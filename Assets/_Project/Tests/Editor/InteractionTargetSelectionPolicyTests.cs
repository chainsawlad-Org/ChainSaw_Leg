using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class InteractionTargetSelectionPolicyTests
{
    private static readonly InteractionTargetSelectionRules DefaultRules =
        new InteractionTargetSelectionRules(22.5f, 45f);

    [Test]
    public void DirectCandidateWinsOverCloserAdjacentCandidate()
    {
        var directCandidate = CreateCandidate(0f, 4f, 2);
        var adjacentCandidate = CreateCandidate(45f, 0.25f, 1);
        var candidates = new List<InteractionTargetCandidate>
        {
            adjacentCandidate,
            directCandidate
        };

        bool hasCandidate = InteractionTargetSelectionPolicy.TrySelectBestCandidate(
            candidates,
            DefaultRules,
            out InteractionTargetCandidate selectedCandidate);

        Assert.That(hasCandidate, Is.True);
        Assert.That(selectedCandidate, Is.SameAs(directCandidate));
    }

    [Test]
    public void NearestCandidateWinsWithinSameDirectionPriority()
    {
        var fartherCandidate = CreateCandidate(45f, 2f, 1);
        var nearerCandidate = CreateCandidate(-45f, 1f, 2);
        var candidates = new List<InteractionTargetCandidate>
        {
            fartherCandidate,
            nearerCandidate
        };

        bool hasCandidate = InteractionTargetSelectionPolicy.TrySelectBestCandidate(
            candidates,
            DefaultRules,
            out InteractionTargetCandidate selectedCandidate);

        Assert.That(hasCandidate, Is.True);
        Assert.That(selectedCandidate, Is.SameAs(nearerCandidate));
    }

    [Test]
    public void CandidateOutsideFacingAndAdjacentDirectionsIsRejected()
    {
        var candidates = new List<InteractionTargetCandidate>
        {
            CreateCandidate(90f, 0.25f, 1),
            CreateCandidate(-90f, 0.25f, 2)
        };

        bool hasCandidate = InteractionTargetSelectionPolicy.TrySelectBestCandidate(
            candidates,
            DefaultRules,
            out InteractionTargetCandidate selectedCandidate);

        Assert.That(hasCandidate, Is.False);
        Assert.That(selectedCandidate, Is.Null);
    }

    [Test]
    public void StableIdBreaksEqualPriorityAndDistanceTie()
    {
        var higherIdCandidate = CreateCandidate(0f, 1f, 20);
        var lowerIdCandidate = CreateCandidate(0f, 1f, 10);
        var candidates = new List<InteractionTargetCandidate>
        {
            higherIdCandidate,
            lowerIdCandidate
        };

        InteractionTargetSelectionPolicy.TrySelectBestCandidate(
            candidates,
            DefaultRules,
            out InteractionTargetCandidate selectedCandidate);

        Assert.That(selectedCandidate, Is.SameAs(lowerIdCandidate));
    }

    [Test]
    public void CentralTwentyTwoAndHalfDegreesHaveDirectPriority()
    {
        var centralCandidate = CreateCandidate(22.4f, 4f, 2);
        var adjacentCandidate = CreateCandidate(22.6f, 0.25f, 1);
        var candidates = new List<InteractionTargetCandidate>
        {
            adjacentCandidate,
            centralCandidate
        };

        InteractionTargetSelectionPolicy.TrySelectBestCandidate(
            candidates,
            DefaultRules,
            out InteractionTargetCandidate selectedCandidate);

        Assert.That(selectedCandidate, Is.SameAs(centralCandidate));
    }

    [TestCase(44.9f, true)]
    [TestCase(45f, true)]
    [TestCase(45.1f, false)]
    public void NinetyDegreeConeBoundaryIsApplied(float angleDegrees, bool expectedSelection)
    {
        var candidates = new List<InteractionTargetCandidate>
        {
            CreateCandidate(angleDegrees, 1f, 1)
        };

        bool hasCandidate = InteractionTargetSelectionPolicy.TrySelectBestCandidate(
            candidates,
            DefaultRules,
            out _);

        Assert.That(hasCandidate, Is.EqualTo(expectedSelection));
    }

    [TestCase(1f, 0f, Direction8.Right)]
    [TestCase(1f, 1f, Direction8.UpRight)]
    [TestCase(0f, 1f, Direction8.Up)]
    [TestCase(-1f, 1f, Direction8.UpLeft)]
    [TestCase(-1f, 0f, Direction8.Left)]
    [TestCase(-1f, -1f, Direction8.DownLeft)]
    [TestCase(0f, -1f, Direction8.Down)]
    [TestCase(1f, -1f, Direction8.DownRight)]
    public void VectorIsConvertedToExpectedDirection(float x, float y, Direction8 expectedDirection)
    {
        Assert.That(Direction8Utility.FromVector(new Vector2(x, y)), Is.EqualTo(expectedDirection));
    }

    private static InteractionTargetCandidate CreateCandidate(
        float angleDegrees,
        float sqrDistance,
        int stableId)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float facingDot = Mathf.Cos(angleRadians);

        return new InteractionTargetCandidate(
            stableId,
            sqrDistance,
            facingDot,
            stableId);
    }
}
