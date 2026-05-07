using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChainSawLeg.Features.Minigames.Snake
{
    public sealed class SnakeBoardPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform boardRoot;
        [SerializeField] private float cellSize = 28f;
        [SerializeField] private float cellOverlap = 1f;
        [SerializeField] private Color emptyColor = new(0f, 0f, 0f, 0f);
        [SerializeField] private Color bodyColor = new(0.27f, 0.83f, 0.43f, 1f);
        [SerializeField] private Color headColor = new(0.55f, 1f, 0.55f, 1f);
        [SerializeField] private Color foodColor = new(1f, 0.3f, 0.3f, 1f);

        private Image[,] cellImages;
        private int boardWidth;
        private int boardHeight;

        public void Initialize(int boardWidth, int boardHeight)
        {
            if (boardRoot == null)
            {
                boardRoot = GetComponent<RectTransform>();
            }

            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;

            BuildBoard();
        }

        public void DrawBoard(IReadOnlyList<Vector2Int> snakeSegments, Vector2Int foodPosition)
        {
            if (cellImages == null)
            {
                return;
            }

            for (int y = 0; y < boardHeight; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    cellImages[x, y].color = emptyColor;
                }
            }

            PaintCell(foodPosition, foodColor);

            for (int index = snakeSegments.Count - 1; index >= 0; index--)
            {
                PaintCell(snakeSegments[index], index == 0 ? headColor : bodyColor);
            }
        }

        private void BuildBoard()
        {
            for (int index = boardRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(boardRoot.GetChild(index).gameObject);
            }

            cellImages = new Image[boardWidth, boardHeight];

            float step = cellSize - cellOverlap;
            float width = boardWidth * cellSize - (boardWidth - 1) * cellOverlap;
            float height = boardHeight * cellSize - (boardHeight - 1) * cellOverlap;

            boardRoot.sizeDelta = new Vector2(width, height);

            for (int y = 0; y < boardHeight; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    GameObject cellObject = new($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                    cellObject.transform.SetParent(boardRoot, false);

                    RectTransform rectTransform = cellObject.GetComponent<RectTransform>();
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                    rectTransform.anchoredPosition = new Vector2(
                        x * step,
                        -y * step);

                    Image image = cellObject.GetComponent<Image>();
                    image.color = emptyColor;

                    cellImages[x, y] = image;
                }
            }
        }

        private void PaintCell(Vector2Int position, Color color)
        {
            if (position.x < 0 || position.x >= boardWidth || position.y < 0 || position.y >= boardHeight)
            {
                return;
            }

            cellImages[position.x, position.y].color = color;
        }
    }
}