using UnityEngine;
using UnityEngine.UI;

public class UIPopupSetting : BasePopup
{
    [Header("Buttons")]
    [SerializeField] private Toggle toggleSound;
    [SerializeField] private Toggle toggleMusic;
    [SerializeField] private Toggle toggleVibration;

    [SerializeField] private Button buttonClose;

    protected override void Awake()
    {
        base.Awake();
        AddListeners();
    }

    void OnEnable()
    {
        LoadUI();
        buttonClose.onClick.AddListener(OnClickButtonClose);
    }

    void OnDisable()
    {
        buttonClose.onClick.RemoveListener(OnClickButtonClose);
    }

    private void LoadUI()
    {
        var data = SettingsManager.Instance.Data;

        toggleSound.isOn = data.isSoundOn;
        toggleMusic.isOn = data.isMusicOn;
        toggleVibration.isOn = data.isVibrationOn;
    }

    private void AddListeners()
    {
        toggleSound.onValueChanged.AddListener(val => SettingsManager.Instance.ToggleSound());
        toggleMusic.onValueChanged.AddListener(val => SettingsManager.Instance.ToggleMusic());
        toggleVibration.onValueChanged.AddListener(val => SettingsManager.Instance.ToggleVibration());
    }

    private void OnClickButtonClose()
    {
        Hide();
    }
}
