using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChainSawLeg.Features.Minigames.Snake
{
    public sealed class SnakeSessionOrchestrator
    {
        private readonly List<Vector2Int> snakeSegments = new();
        private SnakeSessionConfig snakeSessionConfig;
        private SnakeDirection currentDirection;
        private SnakeDirection queuedDirection;
        private Vector2Int foodPosition;
        private float stepTimer;
        private float remainingTime;
        private bool isGameOver;
        private bool isSessionFinished;

        public event Action<IReadOnlyList<Vector2Int>, Vector2Int> BoardChanged;
        public event Action<int> ScoreChanged;
        public event Action<int> SessionFinished;

        public int Score { get; private set; }
        public float RemainingTime => remainingTime;
        public bool IsGameOver => isGameOver;
        public bool IsSessionFinished => isSessionFinished;

        public void Initialize(SnakeSessionConfig snakeSessionConfig)
        {
            this.snakeSessionConfig = snakeSessionConfig;
            RestartSession();
        }

        public void RestartSession()
        {
            if (snakeSessionConfig == null)
            {
                return;
            }

            snakeSegments.Clear();
            Score = 0;
            currentDirection = SnakeDirection.Right;
            queuedDirection = SnakeDirection.Right;
            stepTimer = snakeSessionConfig.StepIntervalSeconds;
            remainingTime = snakeSessionConfig.SessionDurationSeconds;
            isGameOver = false;
            isSessionFinished = false;

            int centerX = snakeSessionConfig.BoardWidth / 2;
            int centerY = snakeSessionConfig.BoardHeight / 2;

            for (int index = 0; index < snakeSessionConfig.StartingLength; index++)
            {
                snakeSegments.Add(new Vector2Int(centerX - index, centerY));
            }

            SpawnFood();
            ScoreChanged?.Invoke(Score);
            BoardChanged?.Invoke(snakeSegments, foodPosition);
        }

        public void QueueDirection(SnakeDirection snakeDirection)
        {
            if (isSessionFinished)
            {
                return;
            }

            if (snakeSegments.Count > 1 && currentDirection.IsOpposite(snakeDirection))
            {
                return;
            }

            queuedDirection = snakeDirection;
        }

        public void Tick(float deltaTime)
        {
            if (snakeSessionConfig == null || isSessionFinished)
            {
                return;
            }

            remainingTime -= deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isSessionFinished = true;
                SessionFinished?.Invoke(Score);
                return;
            }

            stepTimer -= deltaTime;

            if (stepTimer > 0f)
            {
                return;
            }

            stepTimer += snakeSessionConfig.StepIntervalSeconds;
            StepForward();
        }

        private void StepForward()
        {
            currentDirection = queuedDirection;

            Vector2Int nextHead = snakeSegments[0] + currentDirection.ToVector();

            bool isOutsideBoard =
                nextHead.x < 0
                || nextHead.y < 0
                || nextHead.x >= snakeSessionConfig.BoardWidth
                || nextHead.y >= snakeSessionConfig.BoardHeight;

            if (isOutsideBoard || snakeSegments.Contains(nextHead))
            {
                isGameOver = true;
                isSessionFinished = true;
                SessionFinished?.Invoke(Score);
                return;
            }

            snakeSegments.Insert(0, nextHead);

            if (nextHead == foodPosition)
            {
                Score += 1;
                ScoreChanged?.Invoke(Score);
                SpawnFood();
            }
            else
            {
                snakeSegments.RemoveAt(snakeSegments.Count - 1);
            }

            BoardChanged?.Invoke(snakeSegments, foodPosition);
        }

        private void SpawnFood()
        {
            List<Vector2Int> freeCells = new();

            for (int y = 0; y < snakeSessionConfig.BoardHeight; y++)
            {
                for (int x = 0; x < snakeSessionConfig.BoardWidth; x++)
                {
                    Vector2Int candidate = new(x, y);

                    if (!snakeSegments.Contains(candidate))
                    {
                        freeCells.Add(candidate);
                    }
                }
            }

            if (freeCells.Count == 0)
            {
                isSessionFinished = true;
                SessionFinished?.Invoke(Score);
                return;
            }

            int randomIndex = UnityEngine.Random.Range(0, freeCells.Count);
            foodPosition = freeCells[randomIndex];
        }
    }
}