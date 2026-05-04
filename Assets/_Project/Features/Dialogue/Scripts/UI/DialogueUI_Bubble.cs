using UnityEngine;
using TMPro;

public class DialogueUI_Bubble : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0);

    private Transform target;

    private void Awake()
    {
        root.SetActive(false);
    }

    public void SetTarget(Transform t)
    {
        target = t;
        Debug.Log("Bubble target: " + t.name);
    }

    public void ShowRoot()
    {
        root.SetActive(true);
    }

    public void ShowText(string message)
    {
        text.text = message;
        root.SetActive(true);

        // Invoke(nameof(Hide), 3f);
    }

    private void LateUpdate()
    {
        if (target != null && root.activeSelf)
        {
            root.transform.position = target.position + offset;
        }
    }

    public void Show(string message, Transform targetTransform)
    {
        target = targetTransform;
        text.text = message;
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
        target = null;
    }
}
