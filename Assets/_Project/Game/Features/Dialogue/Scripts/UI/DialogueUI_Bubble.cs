using UnityEngine;
using TMPro;

public class DialogueUI_Bubble : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Vector3 offset = new Vector3(1f, 1f, 0);

    private Transform target;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void ShowRoot()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void ShowText(string message)
    {
        if (text != null)
            text.text = message;

        if (root != null)
            root.SetActive(true);

        // Invoke(nameof(Hide), 3f);
    }

    private void LateUpdate()
    {
        if (target != null && root != null && root.activeSelf)
        {
            root.transform.position = target.position + offset;

        }
    }

    public void Show(string message, Transform targetTransform)
    {
        target = targetTransform;

        if (text != null)
            text.text = message;

        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        target = null;
    }
}
