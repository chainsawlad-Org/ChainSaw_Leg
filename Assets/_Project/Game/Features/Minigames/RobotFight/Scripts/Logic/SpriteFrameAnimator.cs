using UnityEngine;

namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Pure sprite-frame animation state for the robot: ping-pong stepping for the hammer
    /// attack, two-frame toggling for the distracted state, and direct frame display.
    /// Timing stays with the owning MonoBehaviour.
    /// </summary>
    public class SpriteFrameAnimator
    {
        private readonly Sprite[] sprites;
        private readonly SpriteRenderer spriteRenderer;
        private int currentFrameIndex;
        private bool handsDirection = true;

        /// <summary>Creates the animator; <paramref name="initialFrameIndex"/> is the index the animation starts from.</summary>
        public SpriteFrameAnimator(Sprite[] sprites, SpriteRenderer spriteRenderer, int initialFrameIndex)
        {
            this.sprites = sprites;
            this.spriteRenderer = spriteRenderer;
            currentFrameIndex = initialFrameIndex;
        }

        /// <summary>Number of frames in the animation.</summary>
        public int FrameCount => sprites.Length;

        /// <summary>Currently displayed (or primed) frame index.</summary>
        public int CurrentFrameIndex => currentFrameIndex;

        /// <summary>Displays the frame at <paramref name="index"/> immediately.</summary>
        public void ShowFrame(int index)
        {
            currentFrameIndex = index;
            spriteRenderer.sprite = sprites[index];
        }

        /// <summary>
        /// Updates the frame index without applying the sprite — used when the pose only
        /// becomes visible through the next animation tick.
        /// </summary>
        public void PrimeFrame(int index)
        {
            currentFrameIndex = index;
        }

        /// <summary>Displays whichever of the two given frames the current index is not (two-frame toggle).</summary>
        public void Toggle(int firstIndex, int secondIndex)
        {
            ShowFrame(currentFrameIndex == firstIndex ? secondIndex : firstIndex);
        }

        /// <summary>
        /// Advances the ping-pong animation one step, bouncing at the array ends.
        /// Returns true when the animation wrapped back to frame 0 — the hammer hit moment.
        /// </summary>
        public bool StepPingPong()
        {
            int nextFrameIndex = handsDirection ? currentFrameIndex + 1 : currentFrameIndex - 1;

            if (nextFrameIndex >= sprites.Length - 1 || nextFrameIndex <= 0)
            {
                handsDirection = !handsDirection;
            }

            currentFrameIndex = nextFrameIndex;

            if (currentFrameIndex < sprites.Length)
            {
                spriteRenderer.sprite = sprites[currentFrameIndex];
            }

            return currentFrameIndex == 0;
        }
    }
}
