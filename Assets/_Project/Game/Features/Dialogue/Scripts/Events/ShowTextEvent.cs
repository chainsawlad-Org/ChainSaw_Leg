using UnityEngine;
using System;

[Serializable]
public class ShowTextEvent : IDialogueEvent
{
    [TextArea(2, 5)]
    public string text;

    public void Execute(DialogueManager manager)
    {
        manager.ShowText(text);

        switch (manager.CurrentType)
        {
            case DialogueType.RPG:

            case DialogueType.Cutscene:
                manager.SetState(DialogueState.WaitingInput);
                break;

            case DialogueType.Bubble:
                manager.SetState(DialogueState.Playing);
                manager.ProcessNextEvent();
                break;
        }
    }
}
