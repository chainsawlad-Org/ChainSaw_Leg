using UnityEngine;
using UnityEngine.UI;

public class HPBarView : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private Unit unit;

    public void Bind(Unit unit)
    {
        this.unit = unit;

        unit.OnHPChanged += UpdateBar;

        UpdateBar(unit.CurrentHP, unit.MaxHP);
    }

    private void UpdateBar(int current, int max)
    {
        fillImage.fillAmount = (float)current / max;
    }

    private void OnDestroy()
    {
        if (unit != null)
            unit.OnHPChanged -= UpdateBar;
    }
}
