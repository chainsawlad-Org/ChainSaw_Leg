using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class DialogueNode
{
    [TextArea(2, 5)]
    public string text;
    public List<DialogueChoice> choices;
    public DialogueNode nextNode;
}
