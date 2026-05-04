using UnityEngine;
using TMPro;

public class DialogueUI_Cutscene : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        root.SetActive(false);
    }

    public void ShowRoot()
    {
        root.SetActive(true);
        Clear();
    }

    public void ShowText(string t)
    {
        root.SetActive(true);
        text.text = t;
    }

    public void Hide()
    {
        root.SetActive(false);
        Clear();
    }

    private void Clear()
    {
        text.text = "";
    }
}