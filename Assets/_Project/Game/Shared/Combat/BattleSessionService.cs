using System;
using System.Collections.Generic;

public sealed class BattleSessionService
{
    private readonly HashSet<string> defeatedEncounterIds = new(StringComparer.Ordinal);

    private string activeEncounterId;
    private string activeReturnSceneName;
    private string pendingReturnSceneName;
    private float activeReturnPositionX;
    private float activeReturnPositionY;
    private float pendingReturnPositionX;
    private float pendingReturnPositionY;
    private bool hasActiveEncounter;
    private bool hasPendingReturnPosition;

    public void BeginEncounter(string encounterId, float returnPositionX, float returnPositionY)
    {
        BeginEncounter(encounterId, returnPositionX, returnPositionY, null);
    }

    public void BeginEncounter(
        string encounterId,
        float returnPositionX,
        float returnPositionY,
        string returnSceneName)
    {
        if (string.IsNullOrWhiteSpace(encounterId))
            throw new ArgumentException("Encounter ID is required.", nameof(encounterId));

        activeEncounterId = encounterId;
        activeReturnSceneName = returnSceneName;
        activeReturnPositionX = returnPositionX;
        activeReturnPositionY = returnPositionY;
        hasActiveEncounter = true;
    }

    public void CompleteCurrentEncounter(bool playerWon)
    {
        if (!hasActiveEncounter)
            return;

        if (playerWon)
            defeatedEncounterIds.Add(activeEncounterId);

        pendingReturnPositionX = activeReturnPositionX;
        pendingReturnPositionY = activeReturnPositionY;
        pendingReturnSceneName = activeReturnSceneName;
        hasPendingReturnPosition = true;
        ClearActiveEncounter();
    }

    public bool IsEncounterDefeated(string encounterId)
    {
        return !string.IsNullOrWhiteSpace(encounterId) && defeatedEncounterIds.Contains(encounterId);
    }

    public bool TryConsumeReturnPosition(out float positionX, out float positionY)
    {
        if (!hasPendingReturnPosition)
        {
            positionX = default;
            positionY = default;
            return false;
        }

        positionX = pendingReturnPositionX;
        positionY = pendingReturnPositionY;
        hasPendingReturnPosition = false;
        return true;
    }

    public bool TryConsumeReturnSceneName(out string sceneName)
    {
        sceneName = pendingReturnSceneName;
        pendingReturnSceneName = null;
        return !string.IsNullOrWhiteSpace(sceneName);
    }

    public void Reset()
    {
        defeatedEncounterIds.Clear();
        ClearActiveEncounter();
        hasPendingReturnPosition = false;
        pendingReturnSceneName = null;
    }

    private void ClearActiveEncounter()
    {
        activeEncounterId = null;
        activeReturnSceneName = null;
        activeReturnPositionX = default;
        activeReturnPositionY = default;
        hasActiveEncounter = false;
    }
}
