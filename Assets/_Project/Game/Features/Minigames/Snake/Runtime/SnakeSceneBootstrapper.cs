using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames.Snake
{
    public sealed class SnakeSceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private SnakeSessionConfig snakeSessionConfig;
        [SerializeField] private SnakeBoardPresenter snakeBoardPresenter;
        [SerializeField] private SnakeHudPresenter snakeHudPresenter;

        private SnakeSessionOrchestrator snakeSessionOrchestrator;
        private bool finishEventRaised;
        private bool isSessionStarted;

        public event Action<int> SessionFinished;
        public event Action ExitRequested;

        private void Start()
        {
            snakeSessionOrchestrator = new SnakeSessionOrchestrator();

            snakeBoardPresenter.Initialize(
                snakeSessionConfig.BoardWidth,
                snakeSessionConfig.BoardHeight);

            snakeHudPresenter.Initialize();

            snakeSessionOrchestrator.BoardChanged += HandleBoardChanged;
            snakeSessionOrchestrator.ScoreChanged += HandleScoreChanged;
            snakeSessionOrchestrator.SessionFinished += HandleSessionFinished;

            snakeSessionOrchestrator.Initialize(snakeSessionConfig);

            ShowStartState();
        }

        private void Update()
        {
            if (snakeSessionOrchestrator == null)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitRequested?.Invoke();
            }

            if (!isSessionStarted)
            {
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    StartSession();
                }

                return;
            }

            RouteInput();
            snakeSessionOrchestrator.Tick(Time.unscaledDeltaTime);

            if (!snakeSessionOrchestrator.IsSessionFinished)
            {
                snakeHudPresenter.ShowRunning(snakeSessionOrchestrator.RemainingTime);
                return;
            }

            if (snakeSessionOrchestrator.IsGameOver)
            {
                snakeHudPresenter.ShowGameOver(
                    snakeSessionOrchestrator.Score,
                    snakeSessionOrchestrator.RemainingTime);
            }
            else
            {
                snakeHudPresenter.ShowCompleted(
                    snakeSessionOrchestrator.Score,
                    snakeSessionOrchestrator.RemainingTime);
            }
            
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                RestartSession();
            }
        }

        private void OnDestroy()
        {
            if (snakeSessionOrchestrator == null)
            {
                return;
            }

            snakeSessionOrchestrator.BoardChanged -= HandleBoardChanged;
            snakeSessionOrchestrator.ScoreChanged -= HandleScoreChanged;
            snakeSessionOrchestrator.SessionFinished -= HandleSessionFinished;
        }

        private void StartSession()
        {
            isSessionStarted = true;
            snakeHudPresenter.HideStartScreen();
            snakeHudPresenter.HideGameOverScreen();
            snakeHudPresenter.ShowRunning(snakeSessionOrchestrator.RemainingTime);
        }

        private void RestartSession()
        {
            finishEventRaised = false;
            isSessionStarted = false;
            snakeSessionOrchestrator.RestartSession();
            ShowStartState();
        }

        private void ShowStartState()
        {
            snakeHudPresenter.ShowStartScreen(snakeSessionOrchestrator.RemainingTime);
        }

        private void RouteInput()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Up);
                }
                else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Down);
                }
                else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Left);
                }
                else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Right);
                }
            }

            if (Gamepad.current != null)
            {
                if (Gamepad.current.dpad.up.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Up);
                }
                else if (Gamepad.current.dpad.down.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Down);
                }
                else if (Gamepad.current.dpad.left.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Left);
                }
                else if (Gamepad.current.dpad.right.wasPressedThisFrame)
                {
                    snakeSessionOrchestrator.QueueDirection(SnakeDirection.Right);
                }
            }
        }

        private void HandleBoardChanged(System.Collections.Generic.IReadOnlyList<Vector2Int> snakeSegments, Vector2Int foodPosition)
        {
            snakeBoardPresenter.DrawBoard(snakeSegments, foodPosition);
        }

        private void HandleScoreChanged(int score)
        {
            snakeHudPresenter.UpdateScore(score);
        }

        private void HandleSessionFinished(int score)
        {
            if (finishEventRaised)
            {
                return;
            }

            finishEventRaised = true;
            SessionFinished?.Invoke(score);
        }
    }
}