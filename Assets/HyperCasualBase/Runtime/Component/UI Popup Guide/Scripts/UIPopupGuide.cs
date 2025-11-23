using UnityEngine;
using UnityEngine.UI;

public class UIPopupGuide : BasePopup
{
    [Header("Buttons")]
    [SerializeField] private Button buttonClose;

    protected override void Awake()
    {
        base.Awake();
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        if (buttonClose != null)
        {
            buttonClose.onClick.AddListener(OnCloseClicked);
        }
    }

    private void OnCloseClicked()
    {
        Hide();
    }
}

