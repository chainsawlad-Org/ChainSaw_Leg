using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

public class ActionTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private CombatEventBus eventBus;
    private Unit targetUnit;
    private bool isSubscribed;

    [Inject]
    public void Construct(CombatEventBus eventBus)
    {
        this.eventBus = eventBus;
        Subscribe();
    }

    public void Bind(Unit unit)
    {
        targetUnit = unit;
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnAction(Unit unit, ActionType action)
    {
        if (unit != targetUnit) return;

        StopAllCoroutines();
        StartCoroutine(Show(action.ToString()));
    }

    private IEnumerator Show(string message)
    {
        text.text = message;
        text.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        text.gameObject.SetActive(false);
    }

    private void Subscribe()
    {
        if (isSubscribed || eventBus == null)
            return;

        eventBus.ActionPerformed += OnAction;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
            return;

        eventBus.ActionPerformed -= OnAction;
        isSubscribed = false;
    }
}
