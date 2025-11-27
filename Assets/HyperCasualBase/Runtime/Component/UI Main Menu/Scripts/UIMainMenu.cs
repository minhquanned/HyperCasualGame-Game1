using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button buttonPlay;
    [SerializeField] private Button buttonSetting;
    [SerializeField] private Button buttonGuide;
    [SerializeField] private Button buttonShop;

    [SerializeField] private UIPopupGuide uIPopupGuide;
    [SerializeField] private UIPopupSetting uIPopupSetting;
    [SerializeField] private UIPopupShop uIPopupShop;

    private void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        if (buttonPlay != null)
        {
            buttonPlay.onClick.AddListener(OnPlayClicked);
        }

        if (buttonSetting != null)
        {
            buttonSetting.onClick.AddListener(OnSettingClicked);
        }

        if (buttonShop != null)
        {
            buttonShop.onClick.AddListener(OnShopClicked);
        }

        if (buttonGuide != null)
        {
            buttonGuide.onClick.AddListener(OnGuideClicked);
        }
    }

    private void OnPlayClicked()
    {
        // load game scene or start game
        SceneManager.LoadScene(1);
        EventBus.Publish(new OnPlayClicked());
    }

    private void OnSettingClicked()
    {
        EventBus.Publish(new OnSettingClicked());
        uIPopupSetting.Show();
    }

    private void OnShopClicked()
    {
        EventBus.Publish(new OnShopClicked());
        uIPopupShop.Show();
    }

    private void OnGuideClicked()
    {
        EventBus.Publish(new OnGuideClicked());
        uIPopupGuide.Show();
    }
}

public struct OnPlayClicked { }
public struct OnSettingClicked { }
public struct OnShopClicked { }
public struct OnGuideClicked { }

