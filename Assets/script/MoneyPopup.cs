using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị popup tiền bay lên khi enemy bị tiêu diệt
/// </summary>
public class MoneyPopup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveUpDistance = 50f; // Khoảng cách bay lên (UI units)
    [SerializeField] private float lifetime = 1.5f; // Thời gian tồn tại

    [Header("References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector3 startPosition;
    private float elapsedTime = 0f;

    private void Awake()
    {
        // Tự động tìm references nếu chưa assign
        if (moneyText == null)
        {
            moneyText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// Hiển thị popup với số tiền
    /// </summary>
    public void Show(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"+{amount}";
        }

        startPosition = transform.localPosition;
        gameObject.SetActive(true);

        StartCoroutine(AnimatePopup());
    }

    /// <summary>
    /// Animation bay lên và mờ dần
    /// </summary>
    private IEnumerator AnimatePopup()
    {
        elapsedTime = 0f;
        Vector3 velocity = Vector3.zero;

        while (elapsedTime < lifetime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / lifetime;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 2f); // Ease out quad
            transform.localPosition = startPosition + Vector3.up * (moveUpDistance * easedProgress);
            // Mờ dần
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - progress;
            }

            yield return null;
        }

        // Ẩn popup sau khi animation xong
        gameObject.SetActive(false);

        // Reset về vị trí ban đầu
        transform.localPosition = startPosition;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
}
