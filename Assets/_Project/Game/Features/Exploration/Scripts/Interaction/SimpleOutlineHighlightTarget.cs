using UnityEngine;

public class SimpleOutlineHighlightTarget : MonoBehaviour, IInteractionHighlightTarget
{
    [SerializeField] private GameObject highlightVisual;

    private void Awake()
    {
        SetHighlighted(false);
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlightVisual != null)
        {
            highlightVisual.SetActive(isHighlighted);
        }
    }
}