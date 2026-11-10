using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    public class MonsterAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Array of sprites to cycle through")]
    public Sprite[] sprites;

    [Tooltip("Time in seconds each sprite stays on screen")]
    public float frameDuration = 0.1f;

    [Tooltip("Should the animation loop?")]
    public bool loop = true;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int currentFrame;
    private bool isPlaying = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"[{name}] MonsterAnimation requires a SpriteRenderer component!");
            enabled = false;
        }
    }

    private void Start()
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[{name}] MonsterAnimation has no sprites assigned!");
            enabled = false;
            return;
        }

        spriteRenderer.sprite = sprites[0];
    }

    private void Update()
    {
        if (!isPlaying || sprites.Length <= 1) return;

        timer += Time.deltaTime;

        // Use a minimum to avoid division/zero issues
        float duration = Mathf.Max(0.001f, frameDuration);

        // Advance frames while enough time has passed
        while (timer >= duration)
        {
            timer -= duration;
            currentFrame++;

            if (currentFrame >= sprites.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = sprites.Length - 1;
                    isPlaying = false;
                }
            }
        }

        spriteRenderer.sprite = sprites[currentFrame];
    }

    // ---- Optional helper methods ----

    public void Play() => isPlaying = true;

    public void Pause() => isPlaying = false;

    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        timer = 0f;
        if (spriteRenderer != null && sprites.Length > 0)
            spriteRenderer.sprite = sprites[0];
    }
}
}
