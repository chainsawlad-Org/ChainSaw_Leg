using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChoiceEvent : IDialogueEvent
{
    public List<DialogueChoice> choices;

    public void Execute(DialogueManager manager)
    {
        if (manager.CurrentType != DialogueType.RPG)
        {
            manager.ProcessNextEvent();
            return;
        }

        manager.ShowChoices(choices);
        manager.SetState(DialogueState.Choosing);
    }
}
