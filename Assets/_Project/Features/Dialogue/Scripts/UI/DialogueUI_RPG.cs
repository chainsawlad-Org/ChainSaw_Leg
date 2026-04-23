using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI_RPG : MonoBehaviour
{
    public static DialogueUI_RPG Instance;

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Continue Hint")]
    [SerializeField] private GameObject continueHint;

    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Show(DialogueNode node)
    {
        root.SetActive(true);
        dialogueText.text = node.text;

        ClearChoices();

        bool hasChoices = node.choices != null && node.choices.Count > 0;
        continueHint.SetActive(!hasChoices);

        if (hasChoices)
        {
            for (int i = 0; i < node.choices.Count; i++)
            {
                int index = i;
                var btn = Instantiate(choiceButtonPrefab, choicesContainer);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = node.choices[i].text;
                btn.onClick.AddListener(() =>
                {
                    DialogueManager.Instance.Choose(index);
                });
            }
        }
    }

    public void Hide()
    {
        root.SetActive(false);
        ClearChoices();
    }

    private void ClearChoices()

    {
        foreach (Transform child in choicesContainer)

        {
            Destroy(child.gameObject);
        }
    }
}
