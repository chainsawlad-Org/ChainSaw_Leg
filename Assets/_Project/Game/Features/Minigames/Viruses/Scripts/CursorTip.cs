using UnityEngine;
using UnityEngine.InputSystem;

namespace ChainSawLeg.Features.Minigames
{
    public class CursorTip : MonoBehaviour
    {
        [SerializeField]
        private Transform cursorTip;
        [SerializeField]
        private float radius;

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;
        private InputAction attackAction;

        private void Awake()
        {
            var map = inputActions.FindActionMap("Player");
            attackAction = map.FindAction("Attack");
        }

        private void OnEnable()
        {
            attackAction.Enable();
            attackAction.performed += OnClickPerformed;
        }

        private void OnDisable()
        {
            attackAction.performed -= OnClickPerformed;
            attackAction.Disable();
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            GameObject hit = CheckObject();
            if (hit != null)
            {
                hit.GetComponent<IClick>().Click();
            }
        }

        private GameObject CheckObject()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(cursorTip.position, radius, LayerMask.GetMask("Interaction"));
            if (hits.Length > 0)
            {
                Debug.Log("Find " + hits[0].name);
                return hits[0].gameObject;
            }
            Debug.Log("Nothing");
            return null;
        }
    }
}
