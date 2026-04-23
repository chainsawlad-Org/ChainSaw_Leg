using UnityEngine;
using TMPro;

public class DialogueUI_Bubble : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private Transform target;

    public static DialogueUI_Bubble Instance;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
            root.transform.position = screenPos;
        }
    }

    public void Show(string message, Transform targetTransform)
    {
        target = targetTransform;
        text.text = message;
        root.SetActive(true);

        Invoke(nameof(Hide), 3f);
    }

    public void Hide()
    {
        root.SetActive(false);
        target = null;
    }
}
