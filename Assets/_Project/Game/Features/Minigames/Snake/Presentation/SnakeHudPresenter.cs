using TMPro;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames.Snake
{
    public sealed class SnakeHudPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private GameObject startScreenRoot;
        [SerializeField] private TMP_Text startScreenText;
        [SerializeField] private GameObject gameOverScreenRoot;
        [SerializeField] private TMP_Text gameOverScreenText;

        public void Initialize()
        {
            UpdateScore(0);
            ShowWaitingToStart();
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void ShowStartScreen(float remainingTime)
        {
            if (stateText != null)
            {
                stateText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
            }

            if (startScreenRoot != null)
            {
                startScreenRoot.SetActive(true);
            }

            if (startScreenText != null)
            {
                startScreenText.text = "Snake\n\nWASD / Arrows — move\nSpace — start";
            }

            if (gameOverScreenRoot != null)
            {
                gameOverScreenRoot.SetActive(false);
            }
        }

        public void HideStartScreen()
        {
            if (startScreenRoot != null)
            {
                startScreenRoot.SetActive(false);
            }
        }

        public void ShowRunning(float remainingTime)
        {
            HideStartScreen();
            HideGameOverScreen();

            if (stateText != null)
            {
                stateText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
            }
        }

        public void ShowGameOver(int score, float remainingTime)
        {
            if (stateText != null)
            {
                stateText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
            }

            if (startScreenRoot != null)
            {
                startScreenRoot.SetActive(false);
            }

            if (gameOverScreenRoot != null)
            {
                gameOverScreenRoot.SetActive(true);
            }

            if (gameOverScreenText != null)
            {
                gameOverScreenText.text = $"Game Over\nScore: {score}\nPress Space";
            }
        }

        public void ShowCompleted(int score, float remainingTime)
        {
            if (stateText != null)
            {
                stateText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
            }

            if (startScreenRoot != null)
            {
                startScreenRoot.SetActive(false);
            }

            if (gameOverScreenRoot != null)
            {
                gameOverScreenRoot.SetActive(true);
            }

            if (gameOverScreenText != null)
            {
                gameOverScreenText.text = $"Time Up\nScore: {score}\nPress Space";
            }
        }

        public void ShowWaitingToStart()
        {
            if (stateText != null)
            {
                stateText.text = string.Empty;
            }
        }

        public void HideGameOverScreen()
        {
            if (gameOverScreenRoot != null)
            {
                gameOverScreenRoot.SetActive(false);
            }
        }
    }
}