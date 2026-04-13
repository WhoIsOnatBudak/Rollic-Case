using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Power-up Buttons")]
    [SerializeField] private Button extraTileButton;
    [SerializeField] private Button undoButton;

    [Header("Power-up Counters")]
    [SerializeField] private TMP_Text extraTileCountText;
    [SerializeField] private TMP_Text undoCountText;

    private bool powerUpsEnabled = false;

    private float remainingTime;
    private bool timerStarted = false;
    private bool timerEnded = false;
    private int currentLevelNumber = 1;

    private void Update()
    {
        if (!timerStarted || timerEnded)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerEnded = true;
            RefreshTimerText();

            if (LevelManager.Instance != null)
                LevelManager.Instance.GameOver();

            return;
        }

        RefreshTimerText();
    }

    public void Initialize(int levelNumber, float startTimeSeconds)
    {
        currentLevelNumber = levelNumber;
        remainingTime = startTimeSeconds;
        timerStarted = false;
        timerEnded = false;
        powerUpsEnabled = false;

        SetPowerUpsVisible(false);
        RefreshLevelText();
        RefreshTimerText();
    }

    public void StartTimerIfNeeded()
    {
        if (timerStarted || timerEnded)
            return;

        timerStarted = true;
        SetPowerUpsVisible(true);
        RefreshPowerUpUI();
    }

    public void StopTimer()
    {
        timerStarted = false;
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    private void RefreshLevelText()
    {
        if (levelText != null)
            levelText.text = "Level " + currentLevelNumber;
    }

    private void RefreshTimerText()
    {
        if (timerText != null)
            timerText.text = "" + Mathf.CeilToInt(remainingTime);
    }

    private void SetPowerUpsVisible(bool value)
    {
        if (extraTileButton   != null) extraTileButton.gameObject.SetActive(value);
        if (undoButton        != null) undoButton.gameObject.SetActive(value);
        if (extraTileCountText != null) extraTileCountText.gameObject.SetActive(value);
        if (undoCountText     != null) undoCountText.gameObject.SetActive(value);
    }

    public void SetPowerUpsEnabled(bool value)
    {
        powerUpsEnabled = value;
        RefreshPowerUpUI();
    }

    private void RefreshPowerUpUI()
    {
        int extraLeft = PowerUpManager.Instance != null ? PowerUpManager.Instance.ExtraTileUsesLeft : 0;
        int undoLeft  = PowerUpManager.Instance != null ? PowerUpManager.Instance.UndoUsesLeft     : 0;

        if (extraTileCountText != null) extraTileCountText.text = extraLeft.ToString();
        if (undoCountText      != null) undoCountText.text      = undoLeft.ToString();

        if (extraTileButton != null) extraTileButton.interactable = powerUpsEnabled && extraLeft > 0;
        if (undoButton      != null) undoButton.interactable      = powerUpsEnabled && undoLeft  > 0;
    }

    public void ShowGameOver()
    {
        SetPowerUpsEnabled(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowLevelComplete()
    {
        SetPowerUpsEnabled(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
    }

    public void OnNextLevelClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnRetryClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnExtraTileButtonClicked()
    {
        PowerUpManager.Instance?.ExecuteExtraTile();
        RefreshPowerUpUI();
    }

    public void OnUndoButtonClicked()
    {
        PowerUpManager.Instance?.ExecuteUndo();
        RefreshPowerUpUI();
    }
}