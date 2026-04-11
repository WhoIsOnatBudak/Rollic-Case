using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;

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

        RefreshLevelText();
        RefreshTimerText();
    }

    public void StartTimerIfNeeded()
    {
        if (timerStarted || timerEnded)
            return;

        timerStarted = true;
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

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowLevelComplete()
    {
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
}