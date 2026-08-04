using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

public class DamageTextView : MonoBehaviour
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

    private void OnHpVisualChanged(Unit unit, int value)
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

    private void Subscribe()
    {
        if (isSubscribed || eventBus == null)
            return;

        eventBus.HpVisualChanged += OnHpVisualChanged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
            return;

        eventBus.HpVisualChanged -= OnHpVisualChanged;
        isSubscribed = false;
    }
}
