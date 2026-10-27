using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Zenject;

namespace ChainSawLeg.Application
{
    public sealed class IntroSequenceController : MonoBehaviour
    {
        private IntroSequenceService introSequenceService;

        [Inject]
        public void Construct(IntroSequenceService service)
        {
            introSequenceService = service;
            Debug.Log("[IntroSequenceController] IntroSequenceService injected.", this);
        }

        [Header("Video Players")]
        [SerializeField] private VideoPlayer firstVideo;
        [SerializeField] private VideoPlayer secondVideo;
        [SerializeField] private VideoPlayer thirdVideo;
        [SerializeField] private VideoPlayer fourthVideo;

        [Header("UI")]
        [SerializeField] private RawImage videoDisplay;
        [SerializeField] private GameObject introText;

        [Header("Timing")]
        [SerializeField] private float textDuration = 10f;

        private void Start()
        {
            Debug.Log(
                $"[IntroSequenceController] Start. Service is null: {introSequenceService == null}",
                this);

            if (introSequenceService == null)
            {
                Debug.LogError(
                    "[IntroSequenceController] IntroSequenceService was not injected.",
                    this);
                return;
            }

            StartCoroutine(PlaySequence());
        }

        private IEnumerator PlaySequence()
        {
            if (introText == null || videoDisplay == null)
            {
                Debug.LogError(
                    "[IntroSequenceController] UI references are missing.",
                    this);
                yield break;
            }

            introText.SetActive(false);
            videoDisplay.gameObject.SetActive(true);

            yield return PlayVideo(firstVideo);
            yield return PlayVideo(secondVideo);
            yield return PlayVideo(thirdVideo);
            yield return PlayVideo(fourthVideo);

            videoDisplay.gameObject.SetActive(false);
            introText.SetActive(true);

            yield return new WaitForSeconds(textDuration);

            introText.SetActive(false);

            Debug.Log("[IntroSequenceController] Intro completed.", this);
            introSequenceService.Complete();
        }

        private IEnumerator PlayVideo(VideoPlayer player)
        {
            if (player == null)
            {
                Debug.LogError(
                    "[IntroSequenceController] VideoPlayer is not assigned.",
                    this);
                yield break;
            }

            player.Stop();
            player.time = 0;
            player.Prepare();

            while (!player.isPrepared)
                yield return null;

            bool finished = false;

            VideoPlayer.EventHandler onFinished = source =>
            {
                finished = true;
            };

            player.loopPointReached += onFinished;
            player.Play();

            while (!finished)
                yield return null;

            player.loopPointReached -= onFinished;
            player.Stop();
        }
    }
}
