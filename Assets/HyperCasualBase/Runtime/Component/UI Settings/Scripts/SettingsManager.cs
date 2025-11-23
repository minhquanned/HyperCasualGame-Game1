using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public SettingsData Data { get; private set; }

    private const string KEY = "GameSettings";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Load();
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void ToggleSound()
    {
        Data.isSoundOn = !Data.isSoundOn;
        Save();
    }

    public void ToggleMusic()
    {
        Data.isMusicOn = !Data.isMusicOn;
        Save();
    }

    public void ToggleVibration()
    {
        Data.isVibrationOn = !Data.isVibrationOn;
        Save();
    }

    public void Save()
    {
        LocalDataManager.Save(Data, KEY);
    }

    public void Load()
    {
        var settingdata = LocalDataManager.Load<SettingsData>(KEY);

        if(settingdata != default)
        {
            Data = settingdata;
        }
        else
        {
            Data = new SettingsData();
        }
    }
}
