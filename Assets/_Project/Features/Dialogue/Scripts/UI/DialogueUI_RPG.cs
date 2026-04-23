using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI_RPG : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choicesButtonPrefab;

    public static DialogueUI_RPG Instance;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Show(DialogueNode node)
    {
        root.SetActive(true);
        text.text = node.text;

        ClearChoices();

        if (node.choices != null && node.choices.Count > 0)
        {
            for (int i = 0; i < node.choices.Count; i++)
            {
                int index = i;

                var btn = Instantiate(choicesButtonPrefab, choicesContainer);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = node.choices[i].text;

                btn.onClick.AddListener(() =>
                {
                    DialogueManager.Instance.Choice(index);
                });
            }
        }
        else
        {
            // если нет вариантов — клик по экрану
        }
    }

    void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);
    }

    public void Hide()
    {
        root.SetActive(false);
        ClearChoices();
    }
}
