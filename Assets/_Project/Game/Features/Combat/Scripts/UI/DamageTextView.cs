using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DamageTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public Unit targetUnit;

    private void OnEnable()
    {
        BattleEvents.OnHPChangedVisual += OnHPChanged;
    }

    private void OnDisable()
    {
        BattleEvents.OnHPChangedVisual -= OnHPChanged;
    }

    private void OnHPChanged(Unit unit, int value)
    {
        if (unit != targetUnit) return;

        StopAllCoroutines();
        StartCoroutine(Show(value));
    }

    private IEnumerator Show(int value)
    {
        text.text = value > 0 ? $"+{value}" : value.ToString();
        text.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        text.gameObject.SetActive(false);
    }
}
