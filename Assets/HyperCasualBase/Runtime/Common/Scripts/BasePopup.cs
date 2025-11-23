using UnityEngine;

public abstract class BasePopup : MonoBehaviour, IPopup
{
    [SerializeField] protected CanvasGroup canvasGroup;

    public bool IsVisible => canvasGroup.alpha > 0.9f;

    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        HideInstant();
    }

    public virtual void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void HideInstant()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
