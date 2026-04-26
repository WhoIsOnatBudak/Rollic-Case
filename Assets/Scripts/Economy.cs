using System;
using UnityEngine;

public static class Economy
{
    private const string InitializedKey = "Economy.Initialized";
    private const string GoldKey = "Economy.Gold";

    public static event Action<int> GoldChanged;

    public static int Gold => GetGold();

    public static void EnsureInitialized()
    {
        if (PlayerPrefs.GetInt(InitializedKey, 0) == 1)
            return;

        PlayerPrefs.SetInt(InitializedKey, 1);

        if (!PlayerPrefs.HasKey(GoldKey))
            PlayerPrefs.SetInt(GoldKey, 0);

        PlayerPrefs.Save();
    }

    public static int GetGold()
    {
        EnsureInitialized();

        int currentGold = PlayerPrefs.GetInt(GoldKey, 0);
        if (currentGold < 0)
        {
            Debug.LogWarning("[Economy] Gold 0'in altina dusmustu. Deger sifirlandi.");
            currentGold = 0;
            PlayerPrefs.SetInt(GoldKey, currentGold);
            PlayerPrefs.Save();
        }

        return currentGold;
    }

    public static void SetGold(int amount)
    {
        EnsureInitialized();

        int previousGold = GetGold();
        int safeAmount = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(GoldKey, safeAmount);
        PlayerPrefs.Save();

        if (previousGold != safeAmount)
            GoldChanged?.Invoke(safeAmount);
    }

    public static void AddGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[Economy] AddGold icin pozitif bir deger bekleniyor.");
            return;
        }

        SetGold(GetGold() + amount);
    }

    public static bool HasEnoughGold(int amount)
    {
        if (amount <= 0)
            return true;

        return GetGold() >= amount;
    }

    public static bool CanAfford(int amount)
    {
        return HasEnoughGold(amount);
    }

    public static bool TrySpendGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[Economy] TrySpendGold negatif deger alamaz.");
            return false;
        }

        if (amount == 0)
            return true;

        if (!HasEnoughGold(amount))
            return false;

        SetGold(GetGold() - amount);
        return true;
    }

    public static bool RemoveGold(int amount)
    {
        return TrySpendGold(amount);
    }

    public static void ResetGold()
    {
        SetGold(0);
    }
}
