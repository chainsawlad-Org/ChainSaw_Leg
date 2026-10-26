using TMPro;
using UnityEngine;

namespace ChainSawLeg.Features.Dialogue
{
    public sealed class DialogueChoiceBlockView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private RectTransform choicesContainer;

        public TextMeshProUGUI QuestionText => questionText;

        public RectTransform ChoicesContainer => choicesContainer;
    }
}