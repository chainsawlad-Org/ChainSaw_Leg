using UnityEngine;

[System.Serializable]
public class TypewriterEvent : IDialogueEvent
{
    [TextArea(2, 5)]
    public string text;
    public float speed = 0.03f;

    public void Execute(DialogueManager manager)
    {
        manager.StartTyping(text, speed);
    }

}