using UnityEngine;

public class DoorGenerator : MonoBehaviour
{
    private const int PixelsPerUnit = 128;
    private const int TileWorldWidthUnits = 2;

    [Header("Door Settings")]
    [SerializeField] private int widthTiles = 3;
    [SerializeField] private int heightTiles = 5;
    [SerializeField] private Color doorColor = new Color(0.55f, 0.27f, 0.07f);

    [ContextMenu("Generate Door 3x5")]
    private void GenerateDoor3x5()
    {
        GenerateDoor(3, 5, new Color(0.55f, 0.27f, 0.07f), "Door_3x5");
    }

    [ContextMenu("Generate Door 2x5")]
    private void GenerateDoor2x5()
    {
        GenerateDoor(2, 5, new Color(0.45f, 0.20f, 0.05f), "Door_2x5");
    }

    [ContextMenu("Generate Door From Current Settings")]
    private void GenerateDoorFromSettings()
    {
        GenerateDoor(widthTiles, heightTiles, doorColor, $"Door_{widthTiles}x{heightTiles}");
    }

    private void GenerateDoor(int targetWidthTiles, int targetHeightTiles, Color color, string objectName)
    {
        int textureWidth = targetWidthTiles * PixelsPerUnit * TileWorldWidthUnits;
        int textureHeight = targetHeightTiles * PixelsPerUnit;

        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color borderColor = Color.black;
        Color handleColor = new Color(0.8f, 0.8f, 0.2f);

        int borderThickness = 4;
        int handleInsetRight = 20;
        int handleWidth = 12;
        float handleBottomNormalized = 0.4f;
        float handleTopNormalized = 0.5f;

        int handleMinX = textureWidth - handleInsetRight;
        int handleMaxX = textureWidth - (handleInsetRight - handleWidth);
        int handleMinY = Mathf.RoundToInt(textureHeight * handleBottomNormalized);
        int handleMaxY = Mathf.RoundToInt(textureHeight * handleTopNormalized);

        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                bool isBorder =
                    x < borderThickness ||
                    x >= textureWidth - borderThickness ||
                    y < borderThickness ||
                    y >= textureHeight - borderThickness;

                bool isHandle =
                    x >= handleMinX &&
                    x < handleMaxX &&
                    y >= handleMinY &&
                    y < handleMaxY;

                if (isBorder)
                {
                    texture.SetPixel(x, y, borderColor);
                }
                else if (isHandle)
                {
                    texture.SetPixel(x, y, handleColor);
                }
                else
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        texture.Apply();

        Sprite doorSprite = Sprite.Create(
            texture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0f),
            PixelsPerUnit);

        GameObject doorObject = new GameObject(objectName);
        SpriteRenderer spriteRenderer = doorObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = doorSprite;

        BoxCollider2D boxCollider = doorObject.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(targetWidthTiles * TileWorldWidthUnits, targetHeightTiles);

        doorObject.transform.position = transform.position;

        Debug.Log($"Door {objectName} created. Size: {targetWidthTiles}x{targetHeightTiles} tiles");
        Debug.Log($"Door position: {doorObject.transform.position}");

#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = doorObject;
#endif
    }
}