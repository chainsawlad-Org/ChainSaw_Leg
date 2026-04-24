using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    public void ShowRoot()
    {
        root.SetActive(true);
    }

    public void ShowText(string t)
    {
        root.SetActive(true);
        dialogueText.text = t;
        ClearChoices();
        continueHint.SetActive(true);
    }

    public void ShowChoices(List<DialogueChoice> choices)
    {
        ClearChoices();

        continueHint.SetActive(false);

        for (int i = 0; i < choices.Count; i++)
        {
            int index = i;
            var btn = Instantiate(choiceButtonPrefab, choicesContainer);

            btn.GetComponentInChildren<TextMeshProUGUI>().text = choices[i].text;
            btn.onClick.AddListener(() =>
            {
                DialogueManager.Instance.Choose(index, choices);
            });
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
            Destroy(child.gameObject);
    }
}
