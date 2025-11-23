using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UILevelSelection : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button buttonBack;

    [Header("Level Buttons")]
    [SerializeField] private Button[] levelButtons;

    [Header("Popups")]
    [SerializeField] private UIPopupSetting popupSetting;

    private void Start()
    {
        InitializeButtons();
        InitializeLevelButtons();
    }

    private void InitializeButtons()
    {
        if (buttonBack != null)
        {
            buttonBack.onClick.AddListener(OnBackClicked);
        }
    }

    private void InitializeLevelButtons()
    {
        if (levelButtons != null && levelButtons.Length > 0)
        {
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i + 1; // Level numbers start from 1
                if (levelButtons[i] != null)
                {
                    levelButtons[i].onClick.AddListener(() => OnLevelSelected(levelIndex));
                }
            }
        }
    }

    private void OnBackClicked()
    {
        // Hide level selection and return to main menu
        gameObject.SetActive(false);
    }

    private void OnLevelSelected(int levelIndex)
    {
        Debug.Log($"Level {levelIndex} selected");
        // Load the selected level
        // SceneManager.LoadScene($"Level_{levelIndex}");
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

