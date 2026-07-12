using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Exploration.Save
{
    public sealed class CheckpointSaveFeedbackView : MonoBehaviour
    {
        private const float DisplaySeconds = 2f;

        private GameObject feedbackRoot;
        private int displayVersion;

        private void Awake()
        {
            feedbackRoot = BuildFeedback();
            feedbackRoot.SetActive(false);
        }

        public async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            int currentVersion = ++displayVersion;
            feedbackRoot.SetActive(true);

            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(DisplaySeconds),
                    ignoreTimeScale: true,
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            finally
            {
                if (currentVersion == displayVersion && feedbackRoot != null)
                    feedbackRoot.SetActive(false);
            }
        }

        private GameObject BuildFeedback()
        {
            var canvasObject = new GameObject(
                "SavedFeedback",
                typeof(RectTransform),
                typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.transform.localPosition = new Vector3(0f, 1.45f, 0f);
            canvasObject.transform.localScale = Vector3.one * 0.01f;

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "Objects";
            canvas.sortingOrder = 20;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(300f, 60f);

            var textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(Text),
                typeof(Outline));
            textObject.transform.SetParent(canvasObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.text = "Сохранено";
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 35;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.75f, 0.97f, 1f);

            var outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            return canvasObject;
        }
    }
}
