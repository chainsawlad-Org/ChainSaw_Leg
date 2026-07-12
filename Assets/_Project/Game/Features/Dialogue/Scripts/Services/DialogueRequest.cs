using System.Collections.Generic;
using UnityEngine;

public class DialogueRequest
{
    public List<IDialogueEvent> Events;
    public DialogueType Type;
    public Transform Speaker;
}