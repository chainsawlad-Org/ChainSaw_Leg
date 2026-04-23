using UnityEngine;
using TMPro;

public class DialogueUI_Cutscene : MonoBehaviour
{
    public static DialogueUI_Cutscene Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Show(DialogueNode node)
    {
        root.SetActive(true);
        text.text = node.text;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
