using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPauseMenu : BasePopup
{
    [Header("Buttons")]
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonRestart;
    [SerializeField] private Button buttonSetting;
    [SerializeField] private Button buttonHome;

    [SerializeField] private UIPopupSetting uIPopupSetting;

    protected override void Awake()
    {
        base.Awake();
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        if (buttonResume != null)
        {
            buttonResume.onClick.AddListener(OnResumeClicked);
        }

        if (buttonRestart != null)
        {
            buttonRestart.onClick.AddListener(OnRestartClicked);
        }

        if (buttonSetting != null)
        {
            buttonSetting.onClick.AddListener(OnSettingClicked);
        }

        if (buttonHome != null)
        {
            buttonHome.onClick.AddListener(OnHomeClicked);
        }
    }

    private void OnResumeClicked()
    {
        AudioManager.Instance?.PlayOneShot(AudioType.UITap);
        EventBus.Publish(new OnResumeClicked());
        Time.timeScale = 1f;
        Hide();
    }

    private void OnRestartClicked()
    {
        AudioManager.Instance?.PlayOneShot(AudioType.UITap);
        EventBus.Publish(new OnRestartClicked());
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSettingClicked()
    {
        AudioManager.Instance?.PlayOneShot(AudioType.UITap);
        EventBus.Publish(new OnPauseSettingClicked());
        uIPopupSetting.Show();
    }

    private void OnHomeClicked()
    {
        AudioManager.Instance?.PlayOneShot(AudioType.UITap);
        EventBus.Publish(new OnHomeClicked());
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public override void Show()
    {
        base.Show();
        Time.timeScale = 0f;
    }
}

public struct OnResumeClicked { }
public struct OnRestartClicked { }
public struct OnPauseSettingClicked { }
public struct OnHomeClicked { }
