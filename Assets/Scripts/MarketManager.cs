using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MarketPowerUpType
{
    ExtraTile,
    Undo
}

public class MarketManager : MonoBehaviour
{
    private const string InitializedKey = "MarketManager.Initialized";
    private const string ExtraTileCountKey = "MarketManager.ExtraTileCount";
    private const string UndoCountKey = "MarketManager.UndoCount";

    [Header("Buttons")]
    [SerializeField] private Button extraTileBuyButton;
    [SerializeField] private Button undoBuyButton;

    [Header("Gold UI")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private string goldPrefix = "Gold: ";

    [Header("Count UI")]
    [SerializeField] private TMP_Text extraTileCountText;
    [SerializeField] private TMP_Text undoCountText;
    [SerializeField] private string extraTileCountPrefix = "Extra Tile: ";
    [SerializeField] private string undoCountPrefix = "Undo: ";

    [Header("Prices")]
    [SerializeField] private int extraTilePrice = 100;
    [SerializeField] private int undoPrice = 100;

    [Header("Feedback")]
    [SerializeField] private Color successFlashColor = Color.yellow;
    [SerializeField] private Color failFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;

    private Coroutine extraTileFlashRoutine;
    private Coroutine undoFlashRoutine;
    private Color extraTileDefaultColor = Color.white;
    private Color undoDefaultColor = Color.white;

    private void Awake()
    {
        Economy.EnsureInitialized();
        EnsureInitialized();

        CacheDefaultColors();

        if (extraTileBuyButton != null)
            extraTileBuyButton.onClick.AddListener(OnExtraTileBuyClicked);

        if (undoBuyButton != null)
            undoBuyButton.onClick.AddListener(OnUndoBuyClicked);
    }

    private void OnEnable()
    {
        Economy.GoldChanged += OnGoldChanged;
        RefreshUI();
    }

    private void OnDisable()
    {
        Economy.GoldChanged -= OnGoldChanged;
    }

    private void OnDestroy()
    {
        if (extraTileBuyButton != null)
            extraTileBuyButton.onClick.RemoveListener(OnExtraTileBuyClicked);

        if (undoBuyButton != null)
            undoBuyButton.onClick.RemoveListener(OnUndoBuyClicked);
    }

    private void Update()
    {
        RefreshUI();
    }

    public void OnExtraTileBuyClicked()
    {
        TryBuy(extraTileBuyButton, MarketPowerUpType.ExtraTile, extraTilePrice);
    }

    public void OnUndoBuyClicked()
    {
        TryBuy(undoBuyButton, MarketPowerUpType.Undo, undoPrice);
    }

    public static void EnsureInitialized()
    {
        if (PlayerPrefs.GetInt(InitializedKey, 0) == 1)
            return;

        PlayerPrefs.SetInt(InitializedKey, 1);

        if (!PlayerPrefs.HasKey(ExtraTileCountKey))
            PlayerPrefs.SetInt(ExtraTileCountKey, 0);

        if (!PlayerPrefs.HasKey(UndoCountKey))
            PlayerPrefs.SetInt(UndoCountKey, 0);

        PlayerPrefs.Save();
    }

    public static int GetPowerUpCount(MarketPowerUpType powerUpType)
    {
        EnsureInitialized();

        string key = GetPowerUpKey(powerUpType);
        int currentCount = PlayerPrefs.GetInt(key, 0);

        if (currentCount < 0)
        {
            currentCount = 0;
            PlayerPrefs.SetInt(key, 0);
            PlayerPrefs.Save();
        }

        return currentCount;
    }

    public static void AddPowerUp(MarketPowerUpType powerUpType, int amount = 1)
    {
        EnsureInitialized();

        if (amount <= 0)
            return;

        string key = GetPowerUpKey(powerUpType);
        int nextAmount = GetPowerUpCount(powerUpType) + amount;
        PlayerPrefs.SetInt(key, nextAmount);
        PlayerPrefs.Save();
    }

    public static bool TryConsumePowerUp(MarketPowerUpType powerUpType, int amount = 1)
    {
        EnsureInitialized();

        if (amount <= 0)
            return true;

        int currentCount = GetPowerUpCount(powerUpType);
        if (currentCount < amount)
            return false;

        string key = GetPowerUpKey(powerUpType);
        PlayerPrefs.SetInt(key, currentCount - amount);
        PlayerPrefs.Save();
        return true;
    }

    private void TryBuy(Button targetButton, MarketPowerUpType powerUpType, int price)
    {
        if (targetButton == null)
            return;

        if (price < 0)
        {
            Debug.LogWarning("[MarketManager] Negatif fiyat kullanilamaz.");
            StartButtonFlash(targetButton, failFlashColor);
            return;
        }

        if (!Economy.CanAfford(price))
        {
            StartButtonFlash(targetButton, failFlashColor);
            return;
        }

        if (!Economy.TrySpendGold(price))
        {
            StartButtonFlash(targetButton, failFlashColor);
            return;
        }

        AddPowerUp(powerUpType, 1);
        StartButtonFlash(targetButton, successFlashColor);
        RefreshUI();
    }

    private void CacheDefaultColors()
    {
        extraTileDefaultColor = GetButtonGraphicColor(extraTileBuyButton, Color.white);
        undoDefaultColor = GetButtonGraphicColor(undoBuyButton, Color.white);
    }

    private void RefreshGoldText()
    {
        if (goldText != null)
            goldText.text = goldPrefix + Economy.GetGold();
    }

    private void RefreshCountTexts()
    {
        if (extraTileCountText != null)
            extraTileCountText.text = extraTileCountPrefix + GetPowerUpCount(MarketPowerUpType.ExtraTile);

        if (undoCountText != null)
            undoCountText.text = undoCountPrefix + GetPowerUpCount(MarketPowerUpType.Undo);
    }

    private void RefreshUI()
    {
        RefreshGoldText();
        RefreshCountTexts();
    }

    private void OnGoldChanged(int currentGold)
    {
        RefreshUI();
    }

    private void StartButtonFlash(Button targetButton, Color flashColor)
    {
        if (targetButton == extraTileBuyButton)
        {
            if (extraTileFlashRoutine != null)
                StopCoroutine(extraTileFlashRoutine);

            extraTileFlashRoutine = StartCoroutine(FlashButtonRoutine(targetButton, extraTileDefaultColor, flashColor));
            return;
        }

        if (targetButton == undoBuyButton)
        {
            if (undoFlashRoutine != null)
                StopCoroutine(undoFlashRoutine);

            undoFlashRoutine = StartCoroutine(FlashButtonRoutine(targetButton, undoDefaultColor, flashColor));
        }
    }

    private IEnumerator FlashButtonRoutine(Button targetButton, Color defaultColor, Color flashColor)
    {
        Graphic buttonGraphic = GetButtonGraphic(targetButton);
        if (buttonGraphic == null)
            yield break;

        buttonGraphic.color = flashColor;
        yield return new WaitForSecondsRealtime(flashDuration);
        buttonGraphic.color = defaultColor;
    }

    private Graphic GetButtonGraphic(Button button)
    {
        if (button == null)
            return null;

        if (button.targetGraphic != null)
            return button.targetGraphic;

        return button.GetComponent<Graphic>();
    }

    private Color GetButtonGraphicColor(Button button, Color fallbackColor)
    {
        Graphic buttonGraphic = GetButtonGraphic(button);
        return buttonGraphic != null ? buttonGraphic.color : fallbackColor;
    }

    private static string GetPowerUpKey(MarketPowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case MarketPowerUpType.ExtraTile:
                return ExtraTileCountKey;
            case MarketPowerUpType.Undo:
                return UndoCountKey;
            default:
                return ExtraTileCountKey;
        }
    }
}
