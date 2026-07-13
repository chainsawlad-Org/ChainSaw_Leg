using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueRuntime
{
    event Action DialogueFinished;

    DialogueState State { get; }
    bool IsActive { get; }

    void StartDialogue(
        IReadOnlyList<IDialogueEvent> events,
        DialogueType type,
        Transform speaker = null);
    void Submit();
    void SelectPreviousChoice();
    void SelectNextChoice();
}
