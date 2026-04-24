using UnityEngine;
using TMPro;

public class DialogueUI_Cutscene : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;

    public void ShowRoot()
    {
        root.SetActive(true);
    }

    public void ShowText(string t)
    {
        root.SetActive(true);
        text.text = t;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}