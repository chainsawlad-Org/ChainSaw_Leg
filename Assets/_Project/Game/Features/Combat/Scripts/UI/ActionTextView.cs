using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ActionTextView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public Unit targetUnit;

    private void OnEnable()
    {
        BattleEvents.OnActionPerfomed += OnAction;
    }

    private void OnDisable()
    {
        BattleEvents.OnActionPerfomed -= OnAction;
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
}
