using UnityEngine;
using System.Collections;

[System.Serializable]
public class DelayEvent : IDialogueEvent
{
    public float duration = 1f;

    public void Execute(DialogueManager manager)
    {
        manager.StartCoroutine(DelayCoroutine(manager));
    }

    private IEnumerator DelayCoroutine(DialogueManager manager)
    {
        yield return new WaitForSeconds(duration);
        manager.ProcessNextEvent();
    }
}