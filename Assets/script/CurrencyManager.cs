using UnityEngine;
using System;
using System.Collections;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public Sprite coinIcon;
    public Sprite gemIcon;

    public int gold = 0;
    public int gem = 0;
    // public int energy = 5;

    // public event Action OnResourceChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        //DontDestroyOnLoad(gameObject);

        // OnResourceChanged += SaveResourcesToDB;
    }

    private void OnDestroy()
    {
        // OnResourceChanged -= SaveResourcesToDB;
    }

    public int Gold => gold;
    public int Gem => gem;
    // public int Energy => energy;

    public void AddGold(int amount)
    {
        gold += amount;
        // OnResourceChanged?.Invoke();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            // OnResourceChanged?.Invoke();
            Debug.Log("Gold spent: " + amount);
            return true;
        }
        // StartCoroutine(ShowFailedPurchasedAfterSeconds(0.2f, "gold"));
        Debug.Log("Not enough gold to spend: " + amount);
        Debug.Log("Your current gold: " + gold);
        return false;
    }

    public void AddGem(int amount)
    {
        gem += amount;
        // OnResourceChanged?.Invoke();
    }

    public bool SpendGem(int amount)
    {
        if (gem >= amount)
        {
            gem -= amount;
            // OnResourceChanged?.Invoke();
            return true;
        }
        // StartCoroutine(ShowFailedPurchasedAfterSeconds(0.2f, "gem"));
        Debug.Log("Not enough gem to spend: " + amount);
        Debug.Log("Your current gem: " + gold);
        return false;
    }

    // public void AddEnergy(int amount)
    // {
    //     energy += amount;
    //     OnResourceChanged?.Invoke();
    // }

    // public bool SpendEnergy(int amount)
    // {
    //     if (energy >= amount)
    //     {
    //         energy -= amount;
    //         OnResourceChanged?.Invoke();
    //         return true;
    //     }
    //     return false;
    // }

    public void SetResources(int newGold, int newGem, bool triggerSave = false)
    {
        gold = newGold;
        gem = newGem;

        if (triggerSave)
        {

        }
        // OnResourceChanged?.Invoke();
    }

    private void SaveResourcesToDB()
    {
        // Debug.Log("[ResourceManager] Saving resources to DB...");
        // FirebaseResource.SaveResources();
    }

    public void ResetResources()
    {
        gold = 0;
        gem = 0;
        // OnResourceChanged?.Invoke();
    }

    // private IEnumerator ShowFailedPurchasedAfterSeconds(float seconds, string resourceType)
    // {
    //     yield return new WaitForSeconds(seconds);

    //     string localizedResourceType = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", $"resource.{resourceType}");

    //     // Lấy message template và thay thế placeholder
    //     string messageTemplate = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "dialog.not_enough_resource");
    //     string localizedMessage = messageTemplate.Replace("{resource}", localizedResourceType);

    //     ConfirmationDialog.Instance.Show(() =>
    //     {
    //         ConfirmationDialog.Instance.Hide();
    //     },
    //     "Dialog.FailedBuy.Title",
    //     localizedMessage,
    //     "Dialog.Default.Confirm",
    //     "Dialog.Default.Cancel",
    //     null,
    //     false,
    //     false);
    // }
}
