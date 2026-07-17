using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10000)]
public sealed class MovementDiagnosticsOverlay : MonoBehaviour
{
    private const float SampleInterval = 0.2f;
    private const float ReferenceLookupInterval = 1f;

    private readonly StringBuilder textBuilder = new(768);

    private PlayerMovement playerMovement;
    private Rigidbody2D playerBody;
    private CameraFlow cameraFlow;
    private GUIStyle labelStyle;

    private float smoothedFrameTime;
    private float currentMaxFrameTime;
    private float previousMaxFrameTime;
    private float maxFrameWindowStartedAt;
    private float nextSampleAt;
    private float nextReferenceLookupAt;
    private int fixedStepsSinceUpdate;
    private int fixedStepsThisFrame;

    private Vector3 previousPlayerRenderPosition;
    private Vector3 previousCameraPosition;
    private float playerRenderDelta;
    private float cameraDelta;
    private bool positionsInitialized;
    private string diagnosticsText = "Movement diagnostics: initializing...";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (FindFirstObjectByType<MovementDiagnosticsOverlay>() != null)
            return;

        var diagnosticsObject = new GameObject("[TEMP] Movement Diagnostics");
        DontDestroyOnLoad(diagnosticsObject);
        diagnosticsObject.AddComponent<MovementDiagnosticsOverlay>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        maxFrameWindowStartedAt = Time.unscaledTime;
        FindReferences();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerMovement = null;
        playerBody = null;
        cameraFlow = null;
        positionsInitialized = false;
        nextReferenceLookupAt = 0f;
    }

    private void FixedUpdate()
    {
        fixedStepsSinceUpdate++;
    }

    private void Update()
    {
        fixedStepsThisFrame = fixedStepsSinceUpdate;
        fixedStepsSinceUpdate = 0;

        float frameTime = Time.unscaledDeltaTime;
        if (frameTime > 0f)
        {
            float smoothing = 1f - Mathf.Exp(-5f * frameTime);
            smoothedFrameTime = smoothedFrameTime <= 0f
                ? frameTime
                : Mathf.Lerp(smoothedFrameTime, frameTime, smoothing);
            currentMaxFrameTime = Mathf.Max(currentMaxFrameTime, frameTime);
        }

        if (Time.unscaledTime - maxFrameWindowStartedAt >= 1f)
        {
            previousMaxFrameTime = currentMaxFrameTime;
            currentMaxFrameTime = 0f;
            maxFrameWindowStartedAt = Time.unscaledTime;
        }

        if ((playerMovement == null || playerBody == null || cameraFlow == null) &&
            Time.unscaledTime >= nextReferenceLookupAt)
        {
            FindReferences();
        }
    }

    private void LateUpdate()
    {
        MeasureMovement();

        if (Time.unscaledTime < nextSampleAt)
            return;

        nextSampleAt = Time.unscaledTime + SampleInterval;
        BuildDiagnosticsText();
    }

    private void FindReferences()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerBody = playerMovement != null
            ? playerMovement.GetComponent<Rigidbody2D>()
            : null;
        cameraFlow = FindFirstObjectByType<CameraFlow>();
        nextReferenceLookupAt = Time.unscaledTime + ReferenceLookupInterval;
        positionsInitialized = false;
    }

    private void MeasureMovement()
    {
        if (playerMovement == null || cameraFlow == null)
        {
            positionsInitialized = false;
            return;
        }

        Vector3 playerPosition = playerMovement.transform.position;
        Vector3 cameraPosition = cameraFlow.transform.position;

        if (positionsInitialized)
        {
            playerRenderDelta = Vector3.Distance(
                playerPosition,
                previousPlayerRenderPosition);
            cameraDelta = Vector3.Distance(
                cameraPosition,
                previousCameraPosition);
        }

        previousPlayerRenderPosition = playerPosition;
        previousCameraPosition = cameraPosition;
        positionsInitialized = true;
    }

    private void BuildDiagnosticsText()
    {
        float fps = smoothedFrameTime > 0f ? 1f / smoothedFrameTime : 0f;
        double refreshRate = Screen.currentResolution.refreshRateRatio.value;
        float fixedRate = Time.fixedDeltaTime > 0f
            ? 1f / Time.fixedDeltaTime
            : 0f;

        textBuilder.Clear();
        textBuilder.AppendLine("TEMP MOVEMENT DIAGNOSTICS");
        textBuilder.Append("FPS ")
            .Append(fps.ToString("F1"))
            .Append(" | frame ")
            .Append((smoothedFrameTime * 1000f).ToString("F2"))
            .Append(" ms | max(1s) ")
            .Append((previousMaxFrameTime * 1000f).ToString("F2"))
            .AppendLine(" ms");
        textBuilder.Append("Display ")
            .Append(refreshRate.ToString("F1"))
            .Append(" Hz | vSync ")
            .Append(QualitySettings.vSyncCount)
            .Append(" | targetFPS ")
            .Append(Application.targetFrameRate)
            .Append(" | ")
            .AppendLine(QualitySettings.names[QualitySettings.GetQualityLevel()]);
        textBuilder.Append("Physics ")
            .Append(fixedRate.ToString("F1"))
            .Append(" Hz (")
            .Append((Time.fixedDeltaTime * 1000f).ToString("F2"))
            .Append(" ms) | steps/frame ")
            .AppendLine(fixedStepsThisFrame.ToString());

        if (playerBody == null || playerMovement == null)
        {
            textBuilder.AppendLine("Player: searching...");
        }
        else
        {
            float renderPhysicsError = Vector2.Distance(
                playerMovement.transform.position,
                playerBody.position);
            textBuilder.Append("Player interpolation: ")
                .Append(playerBody.interpolation)
                .Append(" | velocity ")
                .Append(FormatVector(playerBody.linearVelocity))
                .Append(" | speed ")
                .AppendLine(playerBody.linearVelocity.magnitude.ToString("F2"));
            textBuilder.Append("Player render delta ")
                .Append(playerRenderDelta.ToString("F4"))
                .Append(" | render/physics error ")
                .AppendLine(renderPhysicsError.ToString("F4"));
        }

        if (cameraFlow == null)
        {
            textBuilder.AppendLine("Camera: searching...");
        }
        else
        {
            textBuilder.Append("Camera smooth ")
                .Append(cameraFlow.SmoothTime.ToString("F3"))
                .Append(" s | follow error ")
                .Append(cameraFlow.FollowError.ToString("F4"))
                .Append(" | frame delta ")
                .AppendLine(cameraDelta.ToString("F4"));
            textBuilder.Append("Camera velocity ")
                .Append(FormatVector(cameraFlow.FollowVelocity))
                .Append(" | target ")
                .AppendLine(cameraFlow.Target != null ? cameraFlow.Target.name : "missing");
        }

        textBuilder.Append("Platform ")
            .Append(Application.platform)
            .Append(" | ")
            .Append(Screen.width)
            .Append('x')
            .Append(Screen.height)
            .Append(" | fullscreen ")
            .Append(Screen.fullScreenMode);

        diagnosticsText = textBuilder.ToString();
    }

    private static string FormatVector(Vector2 value)
    {
        return $"({value.x:F2}, {value.y:F2})";
    }

    private static string FormatVector(Vector3 value)
    {
        return $"({value.x:F2}, {value.y:F2}, {value.z:F2})";
    }

    private void OnGUI()
    {
        EnsureStyle();

        float margin = 12f;
        float width = Mathf.Min(720f, Screen.width - margin * 2f);
        float height = labelStyle.lineHeight * 9f + 20f;
        var backgroundRect = new Rect(margin, margin, width, height);

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.82f);
        GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(
            new Rect(
                backgroundRect.x + 10f,
                backgroundRect.y + 8f,
                backgroundRect.width - 20f,
                backgroundRect.height - 16f),
            diagnosticsText,
            labelStyle);
        GUI.color = previousColor;
    }

    private void EnsureStyle()
    {
        int fontSize = Mathf.Clamp(
            Mathf.RoundToInt(Screen.height / 48f),
            14,
            24);

        if (labelStyle != null && labelStyle.fontSize == fontSize)
            return;

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            wordWrap = false
        };
    }
}
