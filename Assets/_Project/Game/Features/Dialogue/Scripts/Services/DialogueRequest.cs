using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public sealed class DialogueRequest
{
    public DialogueRequest(
        IEnumerable<IDialogueEvent> events,
        DialogueType type,
        Transform speaker,
        CancellationToken cancellationToken)
    {
        Events = events?.ToArray() ?? throw new ArgumentNullException(nameof(events));
        Type = type;
        Speaker = speaker;
        CancellationToken = cancellationToken;
    }

    public IReadOnlyList<IDialogueEvent> Events { get; }
    public DialogueType Type { get; }
    public Transform Speaker { get; }
    public CancellationToken CancellationToken { get; }
}
