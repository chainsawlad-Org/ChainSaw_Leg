using UnityEngine;
using System.Collections.Generic;
using System;

public class DialogueNode
{
    [TextArea(2, 5)]
    //public string id;
    public string speaker;
    public string text;
    public string nextId;
}
