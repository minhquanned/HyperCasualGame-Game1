using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển animation của chấm tròn UI hướng dẫn
/// Di chuyển từ vị trí bắt đầu đến vị trí kết thúc với hiệu ứng
/// Sử dụng Coroutine và Lerp built-in của Unity (không cần DOTween)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TutorialHintAnimation : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Image dotImage; // Image component của chấm tròn
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float endScale = 1f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Pulse Effect")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.5f;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Loop Settings")]
    [SerializeField] private bool loopUntilStopped = true; // Lặp lại cho đến khi bị dừng
    [SerializeField] private float pauseBetweenLoops = 0.5f; // Thời gian nghỉ giữa các lần lặp

    private RectTransform rectTransform;
    private Canvas canvas;
    private Camera mainCamera;
    private Action onCompleteCallback;
    private Coroutine animationCoroutine;
    private bool shouldStop = false;
    private Vector2 fromScreenPos;
    private Vector2 toScreenPos;
    private float moveDuration;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Tìm hoặc tạo Canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            // Tạo Canvas nếu chưa có
            GameObject canvasObj = new GameObject("TutorialHintCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Hiển thị trên cùng

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            transform.SetParent(canvasObj.transform, false);
        }

        mainCamera = Camera.main;
    }

    /// <summary>
    /// Khởi tạo và bắt đầu animation
    /// </summary>
    /// <param name="fromWorldPos">Vị trí bắt đầu (world space)</param>
    /// <param name="toWorldPos">Vị trí kết thúc (world space)</param>
    /// <param name="duration">Thời gian di chuyển</param>
    /// <param name="onComplete">Callback khi hoàn thành</param>
    public void Initialize(Vector3 fromWorldPos, Vector3 toWorldPos, float duration, Action onComplete)
    {
        onCompleteCallback = onComplete;
        shouldStop = false;

        // Convert world positions sang screen positions
        fromScreenPos = WorldToCanvasPosition(fromWorldPos);
        toScreenPos = WorldToCanvasPosition(toWorldPos);
        moveDuration = duration;

        // Set vị trí bắt đầu
        rectTransform.anchoredPosition = fromScreenPos;
        rectTransform.localScale = Vector3.one * startScale;

        // Tạo animation
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(PlayAnimationLoopCoroutine());
    }

    /// <summary>
    /// Dừng animation loop
    /// </summary>
    public void StopAnimation()
    {
        shouldStop = true;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // Callback khi dừng
        onCompleteCallback?.Invoke();
    }

    /// <summary>
    /// Chuyển đổi world position sang canvas position
    /// </summary>
    private Vector2 WorldToCanvasPosition(Vector3 worldPos)
    {
        if (mainCamera == null || canvas == null)
        {
            return Vector2.zero;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, worldPos);

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out canvasPos
        );

        return canvasPos;
    }

    /// <summary>
    /// Coroutine để phát animation di chuyển với loop
    /// </summary>
    private IEnumerator PlayAnimationLoopCoroutine()
    {
        while (!shouldStop)
        {
            // Fade in
            if (dotImage != null)
            {
                yield return StartCoroutine(FadeCoroutine(0f, 1f, fadeInDuration));
            }

            if (shouldStop) break;

            // Di chuyển và scale
            yield return StartCoroutine(MoveAndScaleCoroutine(fromScreenPos, toScreenPos, startScale, endScale, moveDuration));

            if (shouldStop) break;

            // Pulse effect
            if (enablePulse)
            {
                yield return StartCoroutine(PulseCoroutine());
            }

            if (shouldStop) break;

            // Fade out
            if (dotImage != null)
            {
                yield return StartCoroutine(FadeCoroutine(1f, 0f, fadeOutDuration));
            }

            if (shouldStop) break;

            // Nếu không loop, thoát
            if (!loopUntilStopped)
            {
                break;
            }

            // Pause giữa các loop
            yield return new WaitForSeconds(pauseBetweenLoops);

            // Reset về vị trí bắt đầu cho loop tiếp theo
            rectTransform.anchoredPosition = fromScreenPos;
            rectTransform.localScale = Vector3.one * startScale;
        }

        // Callback khi hoàn thành
        if (!shouldStop)
        {
            onCompleteCallback?.Invoke();
        }
    }

    /// <summary>
    /// Coroutine fade in/out
    /// </summary>
    private IEnumerator FadeCoroutine(float fromAlpha, float toAlpha, float duration)
    {
        if (dotImage == null) yield break;

        float elapsedTime = 0f;
        Color color = dotImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            dotImage.color = color;

            yield return null;
        }

        // Đảm bảo alpha cuối cùng chính xác
        color.a = toAlpha;
        dotImage.color = color;
    }

    /// <summary>
    /// Coroutine di chuyển và scale
    /// </summary>
    private IEnumerator MoveAndScaleCoroutine(Vector2 fromPos, Vector2 toPos, float fromScale, float toScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Di chuyển với curve
            float moveCurveValue = moveCurve.Evaluate(t);
            rectTransform.anchoredPosition = Vector2.Lerp(fromPos, toPos, moveCurveValue);

            // Scale với curve
            float scaleCurveValue = scaleCurve.Evaluate(t);
            float currentScale = Mathf.Lerp(fromScale, toScale, scaleCurveValue);
            rectTransform.localScale = Vector3.one * currentScale;

            yield return null;
        }

        // Đảm bảo vị trí và scale cuối cùng chính xác
        rectTransform.anchoredPosition = toPos;
        rectTransform.localScale = Vector3.one * toScale;
    }

    /// <summary>
    /// Coroutine pulse effect
    /// </summary>
    private IEnumerator PulseCoroutine()
    {
        float halfDuration = pulseDuration * 0.5f;

        // Scale up
        yield return StartCoroutine(ScaleCoroutine(endScale, endScale * pulseScale, halfDuration, Ease.OutQuad));

        // Scale down
        yield return StartCoroutine(ScaleCoroutine(endScale * pulseScale, endScale, halfDuration, Ease.InQuad));
    }

    /// <summary>
    /// Coroutine scale
    /// </summary>
    private IEnumerator ScaleCoroutine(float fromScale, float toScale, float duration, Ease ease)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Apply easing
            float easedT = ApplyEasing(t, ease);

            float currentScale = Mathf.Lerp(fromScale, toScale, easedT);
            rectTransform.localScale = Vector3.one * currentScale;

            yield return null;
        }

        // Đảm bảo scale cuối cùng chính xác
        rectTransform.localScale = Vector3.one * toScale;
    }

    /// <summary>
    /// Apply easing function
    /// </summary>
    private float ApplyEasing(float t, Ease ease)
    {
        switch (ease)
        {
            case Ease.InQuad:
                return t * t;
            case Ease.OutQuad:
                return t * (2f - t);
            case Ease.InOutQuad:
                return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
            default:
                return t;
        }
    }

    /// <summary>
    /// Enum cho các easing functions
    /// </summary>
    private enum Ease
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad
    }

    private void OnDestroy()
    {
        // Clean up coroutine
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
}
