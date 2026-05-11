using UnityEngine;

public class DoorGenerator : MonoBehaviour
{
    [Header("Настройки двери")] [SerializeField]
    private int widthTiles = 3;

    [SerializeField] private int heightTiles = 5;
    [SerializeField] private Color doorColor = new Color(0.55f, 0.27f, 0.07f);

    [ContextMenu("Создать дверь 3x5")]
    private void GenerateDoor3X5()
    {
        GenerateDoor(3, 5, new Color(0.55f, 0.27f, 0.07f), "Door_3x5");
    }

    [ContextMenu("Создать дверь 2x5")]
    private void GenerateDoor2X5()
    {
        GenerateDoor(2, 5, new Color(0.45f, 0.20f, 0.05f), "Door_2x5");
    }

    [ContextMenu("Создать дверь с текущими настройками")]
    private void GenerateDoorFromSettings()
    {
        GenerateDoor(widthTiles, heightTiles, doorColor, $"Door_{widthTiles}x{heightTiles}");
    }

    private void GenerateDoor(int tilesWidth, int tilesHeight, Color color, string doorName)
    {
        int ppu = 128;
        int widthPx = tilesWidth * ppu * 2; // 2 юнита на тайл
        int heightPx = tilesHeight * ppu;

        Texture2D texture = new Texture2D(widthPx, heightPx);

        // Заполняем основным цветом
        for (int x = 0; x < widthPx; x++)
        {
            for (int y = 0; y < heightPx; y++)
            {
                // Добавляем рамку
                bool isBorder = x < 4 || x > widthPx - 4 || y < 4 || y > heightPx - 4;
                // Добавляем ручку (справа, на высоте 40% от низа)
                bool isHandle = x > widthPx - 20 && x < widthPx - 8 &&
                                y > heightPx * 0.4f && y < heightPx * 0.5f;

                if (isBorder)
                    texture.SetPixel(x, y, Color.black);
                else if (isHandle)
                    texture.SetPixel(x, y, new Color(0.8f, 0.8f, 0.2f));
                else
                    texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Point;

        // Создаем спрайт
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, widthPx, heightPx),
            new Vector2(0.5f, 0f), ppu);

        // Создаем объект на сцене
        GameObject door = new GameObject(doorName);
        SpriteRenderer sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // Добавляем коллайдер
        BoxCollider2D boxCollider = door.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(tilesWidth * 2f, tilesHeight);

        // Позиционируем
        door.transform.position = transform.position;

        Debug.Log($"Дверь {doorName} создана! Размер: {tilesWidth}x{tilesHeight} тайлов");
        Debug.Log($"Позиция двери: {door.transform.position}");

        // Выделяем созданную дверь в сцене
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = door;
#endif
    }
}