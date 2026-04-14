using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorPanel : MonoBehaviour
{
    // ── Inspector referansları ────────────────────────────────────────────────

    [Header("Width")]
    [SerializeField] private Button widthPlusButton;
    [SerializeField] private Button widthMinusButton;
    [SerializeField] private TMP_Text widthText;

    [Header("Height")]
    [SerializeField] private Button heightPlusButton;
    [SerializeField] private Button heightMinusButton;
    [SerializeField] private TMP_Text heightText;

    [Header("Time")]
    [SerializeField] private Button timePlusButton;
    [SerializeField] private Button timeMinusButton;
    [SerializeField] private TMP_Text timeText;

    [Header("Waiting Area")]
    [SerializeField] private Button waitingPlusButton;
    [SerializeField] private Button waitingMinusButton;
    [SerializeField] private TMP_Text waitingText;

    [Header("Panel Kontrolü")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button goBackButton;
    [SerializeField] private LevelSelectPanel levelSelectPanel;

    [Header("Bus Queue")]
    [SerializeField] private Button busQueueButton;
    [SerializeField] private GameObject busQueuePanel;

    [Header("Tile Editor")]
    [SerializeField] private Button editTilesButton;
    [SerializeField] private GameObject tileEditorPanel;

    // ── Limitler ──────────────────────────────────────────────────────────────

    private const int MinSize        = 2;
    private const int MaxSize        = 6;
    private const int TimeStep       = 5;
    private const int MinTime        = 5;
    private const int MaxTime        = 100;
    private const int MinWaiting     = 1;
    private const int MaxWaiting     = 10;

    // ── Aktif level verisi ────────────────────────────────────────────────────

    public LevelData CurrentLevel { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        if (LevelSelectPanel.IsNewLevel)
            InitNewLevel();
        else
            LoadSelectedLevel();

        BindButtons();
        RefreshUI();
    }

    private void OnDisable()
    {
        UnbindButtons();
    }

    // ── Init ──────────────────────────────────────────────────────────────────

    private void InitNewLevel()
    {
        CurrentLevel = new LevelData
        {
            levelNumber       = NextLevelNumber(),
            timerSeconds      = 30f,
            gridWidth         = 3,
            gridHeight        = 3,
            waitingAreaLength = 3,
            buses             = new BusData[0],
            grid              = BuildEmptyGrid(3, 3)
        };
    }

    private void LoadSelectedLevel()
    {
        CurrentLevel = LevelSelectPanel.SelectedLevel;

        if (CurrentLevel == null)
        {
            Debug.LogWarning("[LevelEditorPanel] Seçili level null, boş level açılıyor.");
            InitNewLevel();
        }
    }

    // ── Buton bağlama ─────────────────────────────────────────────────────────

    private void BindButtons()
    {
        widthPlusButton?.onClick.AddListener(OnWidthPlus);
        widthMinusButton?.onClick.AddListener(OnWidthMinus);
        heightPlusButton?.onClick.AddListener(OnHeightPlus);
        heightMinusButton?.onClick.AddListener(OnHeightMinus);
        timePlusButton?.onClick.AddListener(OnTimePlus);
        timeMinusButton?.onClick.AddListener(OnTimeMinus);
        waitingPlusButton?.onClick.AddListener(OnWaitingPlus);
        waitingMinusButton?.onClick.AddListener(OnWaitingMinus);
        saveButton?.onClick.AddListener(OnSaveClicked);
        goBackButton?.onClick.AddListener(OnGoBackClicked);
        busQueueButton?.onClick.AddListener(OnBusQueueClicked);
        editTilesButton?.onClick.AddListener(OnEditTilesClicked);
    }

    private void UnbindButtons()
    {
        widthPlusButton?.onClick.RemoveListener(OnWidthPlus);
        widthMinusButton?.onClick.RemoveListener(OnWidthMinus);
        heightPlusButton?.onClick.RemoveListener(OnHeightPlus);
        heightMinusButton?.onClick.RemoveListener(OnHeightMinus);
        timePlusButton?.onClick.RemoveListener(OnTimePlus);
        timeMinusButton?.onClick.RemoveListener(OnTimeMinus);
        waitingPlusButton?.onClick.RemoveListener(OnWaitingPlus);
        waitingMinusButton?.onClick.RemoveListener(OnWaitingMinus);
        saveButton?.onClick.RemoveListener(OnSaveClicked);
        goBackButton?.onClick.RemoveListener(OnGoBackClicked);
        busQueueButton?.onClick.RemoveListener(OnBusQueueClicked);
        editTilesButton?.onClick.RemoveListener(OnEditTilesClicked);
    }

    // ── Buton callback'leri ───────────────────────────────────────────────────

    private void OnWidthPlus()
    {
        if (CurrentLevel.gridWidth >= MaxSize) return;
        CurrentLevel.gridWidth++;
        RebuildGrid();
        RefreshUI();
    }

    private void OnWidthMinus()
    {
        if (CurrentLevel.gridWidth <= MinSize) return;
        CurrentLevel.gridWidth--;
        RebuildGrid();
        RefreshUI();
    }

    private void OnHeightPlus()
    {
        if (CurrentLevel.gridHeight >= MaxSize) return;
        CurrentLevel.gridHeight++;
        RebuildGrid();
        RefreshUI();
    }

    private void OnHeightMinus()
    {
        if (CurrentLevel.gridHeight <= MinSize) return;
        CurrentLevel.gridHeight--;
        RebuildGrid();
        RefreshUI();
    }

    private void OnTimePlus()
    {
        if (CurrentLevel.timerSeconds >= MaxTime) return;
        CurrentLevel.timerSeconds += TimeStep;
        RefreshUI();
    }

    private void OnTimeMinus()
    {
        if (CurrentLevel.timerSeconds <= MinTime) return;
        CurrentLevel.timerSeconds -= TimeStep;
        RefreshUI();
    }

    private void OnWaitingPlus()
    {
        if (CurrentLevel.waitingAreaLength >= MaxWaiting) return;
        CurrentLevel.waitingAreaLength++;
        RefreshUI();
    }

    private void OnWaitingMinus()
    {
        if (CurrentLevel.waitingAreaLength <= MinWaiting) return;
        CurrentLevel.waitingAreaLength--;
        RefreshUI();
    }

    // ── UI güncelle ───────────────────────────────────────────────────────────

    private void RefreshUI()
    {
        if (widthText   != null) widthText.text   = CurrentLevel.gridWidth.ToString();
        if (heightText  != null) heightText.text  = CurrentLevel.gridHeight.ToString();
        if (timeText    != null) timeText.text    = $"{(int)CurrentLevel.timerSeconds}s";
        if (waitingText != null) waitingText.text = CurrentLevel.waitingAreaLength.ToString();

        // Limit butonları devre dışı bırak
        if (widthPlusButton)   widthPlusButton.interactable   = CurrentLevel.gridWidth  < MaxSize;
        if (widthMinusButton)  widthMinusButton.interactable  = CurrentLevel.gridWidth  > MinSize;
        if (heightPlusButton)  heightPlusButton.interactable  = CurrentLevel.gridHeight < MaxSize;
        if (heightMinusButton) heightMinusButton.interactable = CurrentLevel.gridHeight > MinSize;
        if (timePlusButton)    timePlusButton.interactable    = CurrentLevel.timerSeconds < MaxTime;
        if (timeMinusButton)   timeMinusButton.interactable   = CurrentLevel.timerSeconds > MinTime;
        if (waitingPlusButton)  waitingPlusButton.interactable  = CurrentLevel.waitingAreaLength < MaxWaiting;
        if (waitingMinusButton) waitingMinusButton.interactable = CurrentLevel.waitingAreaLength > MinWaiting;
    }

    // ── Grid yeniden oluştur (boyut değişince) ────────────────────────────────

    private void RebuildGrid()
    {
        // Eski hücreleri bir dictionary'e al — içerik kaybolmasın
        var old = new System.Collections.Generic.Dictionary<(int, int), GridCellData>();
        if (CurrentLevel.grid != null)
            foreach (var cell in CurrentLevel.grid)
                old[(cell.x, cell.y)] = cell;

        CurrentLevel.grid = BuildEmptyGrid(CurrentLevel.gridWidth, CurrentLevel.gridHeight);

        // Sığan eski içerikleri geri koy
        foreach (var cell in CurrentLevel.grid)
        {
            if (old.TryGetValue((cell.x, cell.y), out GridCellData prev))
            {
                cell.contentType = prev.contentType;
                cell.color       = prev.color;
                cell.direction   = prev.direction;
                cell.spawnColors = prev.spawnColors;
            }
        }
    }

    // ── Bus Queue ────────────────────────────────────────────────────────────

    private void OnBusQueueClicked()
    {
        busQueuePanel?.SetActive(!busQueuePanel.activeSelf);
    }

    // ── Edit Tiles ────────────────────────────────────────────────────────────

    private void OnEditTilesClicked()
    {
        tileEditorPanel?.SetActive(!tileEditorPanel.activeSelf);
    }

    // ── Save / Go Back ────────────────────────────────────────────────────────

    private void OnSaveClicked()
    {
        SaveLevel();
    }

    private void OnGoBackClicked()
    {
        levelSelectPanel?.ReturnToList();
    }

    // ── Kaydet ───────────────────────────────────────────────────────────────

    public void SaveLevel()
    {
        string fileName = $"level{CurrentLevel.levelNumber}.json";
        string savePath = Path.Combine(Application.dataPath, "Levels", fileName);

        File.WriteAllText(savePath, JsonUtility.ToJson(CurrentLevel, true));
        Debug.Log($"[LevelEditorPanel] Kaydedildi: {savePath}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // ── Yardımcılar ───────────────────────────────────────────────────────────

    private static int NextLevelNumber()
    {
        string folder = Path.Combine(Application.dataPath, "Levels");
        if (!Directory.Exists(folder)) return 1;

        int max = Directory.GetFiles(folder, "*.json")
                           .Select(f => JsonUtility.FromJson<LevelData>(File.ReadAllText(f))?.levelNumber ?? 0)
                           .DefaultIfEmpty(0)
                           .Max();
        return max + 1;
    }

    private static GridCellData[] BuildEmptyGrid(int w, int h)
    {
        var cells = new GridCellData[w * h];
        int i = 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                cells[i++] = new GridCellData { x = x, y = y, contentType = "Empty" };
        return cells;
    }
}
