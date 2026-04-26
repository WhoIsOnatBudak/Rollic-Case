using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text StartText;

    [Header("Bottom Buttons")]
    [SerializeField] private Button marketButton;
    [SerializeField] private Button menuButton;

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject marketPanel;

    [Header("Default State")]
    [SerializeField] private bool openMarketFirst = false;

    private bool isMarketOpen;

    private void Awake()
    {
        if (marketButton != null)
            marketButton.onClick.AddListener(OnMarketButtonClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    private void Start()
    {
        RefreshStartText();
        SetActiveScreen(openMarketFirst);
    }

    private void OnDestroy()
    {
        if (marketButton != null)
            marketButton.onClick.RemoveListener(OnMarketButtonClicked);

        if (menuButton != null)
            menuButton.onClick.RemoveListener(OnMenuButtonClicked);
    }

    public void OnStartClicked()
    {
        if (PlayerPrefs.GetInt("CurrentLevel", 1) > 10)
            return;

        SceneManager.LoadScene(1);
    }

    public void OnResetClicked()
    {
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        RefreshStartText();
    }

    public void OnMarketButtonClicked()
    {
        SetActiveScreen(true);
    }

    public void OnMenuButtonClicked()
    {
        SetActiveScreen(false);
    }

    private void SetActiveScreen(bool showMarket)
    {
        isMarketOpen = showMarket;

        if (menuPanel != null)
            menuPanel.SetActive(!showMarket);

        if (marketPanel != null)
            marketPanel.SetActive(showMarket);

        RefreshButtonStates();
    }

    private void RefreshStartText()
    {
        if (StartText == null)
            return;

        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        StartText.text = currentLevel > 10 ? "You Win!" : "Level " + currentLevel;
    }

    private void RefreshButtonStates()
    {
        if (menuButton != null)
            menuButton.interactable = isMarketOpen;

        if (marketButton != null)
            marketButton.interactable = !isMarketOpen;
    }
}
