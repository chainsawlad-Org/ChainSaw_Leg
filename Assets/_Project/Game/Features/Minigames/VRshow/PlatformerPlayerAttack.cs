using ChainSawLeg.Features.Exploration;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class PlatformerPlayerAttack : MonoBehaviour
    {
        [SerializeField] private Sprite attackSprite;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private PlatformerPlayerMovement movement;
        [SerializeField] private CameraFlow cameraFlow;
        [SerializeField] private float attackDuration = 0.5f;
        [SerializeField] private float shakeStrength = 0.2f;
        [SerializeField] private int shakeVibrato = 100;
        [SerializeField] private float shakeDuration = 0.4f;

        private Sprite defaultSprite;
        private bool isAttacking;

        private void Awake()
        {
            defaultSprite = sprite.sprite;
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && !isAttacking)
                Attack();
        }

        private void Attack()
        {
            isAttacking = true;
            sprite.sprite = attackSprite;
            movement.LockMovement();
            cameraFlow.ShakeEffect(shakeStrength, shakeVibrato, shakeDuration);

            DOVirtual.DelayedCall(attackDuration, EndAttack);
        }

        private void EndAttack()
        {
            isAttacking = false;
            sprite.sprite = defaultSprite;
            movement.UnlockMovement();
        }
    }
}
