using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    private enum DoorMotionMode
    {
        Rotate,
        Slide,
        MirrorLeftSide
    }

    [Header("Prompt")]
    [SerializeField] private string closedPrompt = "Press E to open";
    [SerializeField] private string openedPrompt = "Press E to close";
    [SerializeField] private bool isLocked;
    [SerializeField] private bool isOpen;

    [Header("Visual Target")]
    [SerializeField] private Transform doorVisual;
    [SerializeField] private bool useLocalSpace = true;

    [Header("Motion")]
    [SerializeField] private DoorMotionMode doorMotionMode = DoorMotionMode.Rotate;
    [SerializeField] private float motionDuration = 0.18f;

    [Header("Rotate")]
    [SerializeField] private float openedAngle = 90f;
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;

    [Header("Slide")]
    [SerializeField] private Vector3 openedOffset = new Vector3(0.8f, 0f, 0f);

    [Header("Mirror Left Side")]
    [SerializeField] private bool mirrorScaleX = true;

    [Header("Optional Animator Fallback")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";

    [Header("Events")]
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;
    [SerializeField] private UnityEvent onLockedInteraction;
    
    [Header("Blocking")]
    [SerializeField] private Collider2D[] blockingColliders;
    [SerializeField] private bool disableCollidersWhileMoving = true;

    private bool isMoving;
    private bool movingTowardOpen;
    private float motionTimer;

    private Quaternion closedLocalRotation;
    private Quaternion openedLocalRotation;
    private Quaternion closedWorldRotation;
    private Quaternion openedWorldRotation;

    private Vector3 closedLocalPosition;
    private Vector3 openedLocalPosition;
    private Vector3 closedWorldPosition;
    private Vector3 openedWorldPosition;

    private Vector3 closedLocalScale;
    private Vector3 openedLocalScale;
    private Vector3 closedWorldScale;
    private Vector3 openedWorldScale;

    private void Awake()
    {
        if (doorVisual == null)
        {
            doorVisual = transform;
        }

        if ((blockingColliders == null || blockingColliders.Length == 0))
        {
            blockingColliders = GetComponentsInChildren<Collider2D>(true);
        }

        CacheClosedState();
        BuildOpenedState();
        ApplyInstantState(isOpen);
        SetBlockingCollidersEnabled(true);
    }

    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
    private void Update()
    {
        if (!isMoving)
        {
            return;
        }

        motionTimer += Time.deltaTime;
        float normalizedTime = motionDuration > 0f
            ? Mathf.Clamp01(motionTimer / motionDuration)
            : 1f;

        float t = movingTowardOpen ? normalizedTime : 1f - normalizedTime;
        ApplyInterpolatedState(t);

        if (normalizedTime >= 1f)
        {
            isMoving = false;
            isOpen = movingTowardOpen;
            ApplyInstantState(isOpen);

            SetBlockingCollidersEnabled(true);

            if (isOpen)
            {
                onOpened?.Invoke();
            }
            else
            {
                onClosed?.Invoke();
            }
        }
    }

    public string GetInteractionPrompt()
    {
        if (isLocked)
        {
            return "Locked";
        }

        return isOpen ? openedPrompt : closedPrompt;
    }

    public bool CanInteract()
    {
        return !isMoving;
    }

    public void Interact()
    {
        if (isLocked)
        {
            onLockedInteraction?.Invoke();
            return;
        }

        if (isMoving)
        {
            return;
        }

        movingTowardOpen = !isOpen;
        motionTimer = 0f;
        isMoving = true;

        if (disableCollidersWhileMoving)
        {
            SetBlockingCollidersEnabled(false);
        }
    }

    public void SetLocked(bool value)
    {
        isLocked = value;
    }

    public void SetOpen(bool value)
    {
        isOpen = value;
        isMoving = false;
        ApplyInstantState(isOpen);
        SetBlockingCollidersEnabled(true);
    }

    private void CacheClosedState()
    {
        closedLocalRotation = doorVisual.localRotation;
        closedWorldRotation = doorVisual.rotation;

        closedLocalPosition = doorVisual.localPosition;
        closedWorldPosition = doorVisual.position;

        closedLocalScale = doorVisual.localScale;
        closedWorldScale = doorVisual.lossyScale;
    }

    private void BuildOpenedState()
    {
        if (doorMotionMode == DoorMotionMode.Rotate)
        {
            Quaternion deltaRotation = Quaternion.AngleAxis(openedAngle, rotationAxis.normalized);

            openedLocalRotation = closedLocalRotation * deltaRotation;
            openedWorldRotation = closedWorldRotation * deltaRotation;

            openedLocalPosition = closedLocalPosition;
            openedWorldPosition = closedWorldPosition;

            openedLocalScale = closedLocalScale;
            openedWorldScale = closedWorldScale;
            return;
        }

        if (doorMotionMode == DoorMotionMode.Slide)
        {
            openedLocalRotation = closedLocalRotation;
            openedWorldRotation = closedWorldRotation;

            openedLocalPosition = closedLocalPosition + openedOffset;
            openedWorldPosition = closedWorldPosition + openedOffset;

            openedLocalScale = closedLocalScale;
            openedWorldScale = closedWorldScale;
            return;
        }

        openedLocalRotation = closedLocalRotation;
        openedWorldRotation = closedWorldRotation;

        openedLocalPosition = new Vector3(
            -closedLocalPosition.x,
            closedLocalPosition.y,
            closedLocalPosition.z);

        openedWorldPosition = new Vector3(
            -closedWorldPosition.x,
            closedWorldPosition.y,
            closedWorldPosition.z);

        openedLocalScale = closedLocalScale;
        openedWorldScale = closedWorldScale;

        if (mirrorScaleX)
        {
            openedLocalScale = new Vector3(
                -closedLocalScale.x,
                closedLocalScale.y,
                closedLocalScale.z);

            openedWorldScale = new Vector3(
                -closedWorldScale.x,
                closedWorldScale.y,
                closedWorldScale.z);
        }
    }

    private void ApplyInstantState(bool opened)
    {
        ApplyInterpolatedState(opened ? 1f : 0f);
    }

    private void ApplyInterpolatedState(float t)
    {
        if (useLocalSpace)
        {
            doorVisual.localRotation = Quaternion.Slerp(closedLocalRotation, openedLocalRotation, t);
            doorVisual.localPosition = Vector3.Lerp(closedLocalPosition, openedLocalPosition, t);
            doorVisual.localScale = Vector3.Lerp(closedLocalScale, openedLocalScale, t);
            return;
        }

        doorVisual.rotation = Quaternion.Slerp(closedWorldRotation, openedWorldRotation, t);
        doorVisual.position = Vector3.Lerp(closedWorldPosition, openedWorldPosition, t);
    }
    
    private void SetBlockingCollidersEnabled(bool isEnabled)
    {
        if (!disableCollidersWhileMoving || blockingColliders == null)
        {
            return;
        }

        foreach (var t in blockingColliders)
        {
            if (t is not null)
            {
                t.enabled = isEnabled;
            }
        }
    }
}