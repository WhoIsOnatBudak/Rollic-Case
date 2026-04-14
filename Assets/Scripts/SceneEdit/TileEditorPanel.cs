using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grid tile editörü. BusQueuePanel ile aynı pattern:
/// inspector'dan referans al, sadece mantık burada.
/// </summary>
public class TileEditorPanel : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Referans")]
    [SerializeField] private LevelEditorPanel editorPanel;
    [SerializeField] private Button           closeButton;

    [Header("Tile Grid")]
    [SerializeField] private Transform gridContainer;   // GridLayoutGroup olan obje

    [Header("Content Picker")]
    [SerializeField] private GameObject contentPickerPanel;
    [SerializeField] private Button     cpEmptyBtn;
    [SerializeField] private Button     cpObstacleBtn;
    [SerializeField] private Button     cpPassengerBtn;
    [SerializeField] private Button     cpSpawnerBtn;
    [SerializeField] private Button     cpCancelBtn;

    [Header("Color Picker")]
    [SerializeField] private GameObject colorPickerPanel;
    [SerializeField] private Button     clrRedBtn;
    [SerializeField] private Button     clrBlueBtn;
    [SerializeField] private Button     clrGreenBtn;
    [SerializeField] private Button     clrYellowBtn;
    [SerializeField] private Button     clrCancelBtn;

    [Header("Spawner Config")]
    [SerializeField] private GameObject spawnerConfigPanel;
    [SerializeField] private Button     spDirUp;
    [SerializeField] private Button     spDirDown;
    [SerializeField] private Button     spDirLeft;
    [SerializeField] private Button     spDirRight;
    [SerializeField] private Button     spAddRed;
    [SerializeField] private Button     spAddBlue;
    [SerializeField] private Button     spAddGreen;
    [SerializeField] private Button     spAddYellow;
    [SerializeField] private Button     spDeleteBtn;
    [SerializeField] private Button     spConfirmBtn;
    [SerializeField] private Button     spCancelBtn;
    [SerializeField] private TMP_Text   spDirectionLabel;
    [SerializeField] private TMP_Text   spQueueLabel;

    // ── Sabitler ──────────────────────────────────────────────────────────────

    private const int SpawnerMin = 1;
    private const int SpawnerMax = 3;

    // ── Renkler ───────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, Color> ColorMap = new()
    {
        { "Red",    new Color(0.90f, 0.22f, 0.22f) },
        { "Blue",   new Color(0.22f, 0.45f, 0.90f) },
        { "Green",  new Color(0.20f, 0.78f, 0.32f) },
        { "Yellow", new Color(1.00f, 0.88f, 0.10f) },
    };

    private static readonly Color ColEmpty    = new Color(0f, 0.78f, 0.78f);
    private static readonly Color ColObstacle = new Color(0.38f, 0.38f, 0.38f);
    private static readonly Color ColSpawner  = new Color(1.00f, 0.55f, 0.00f);

    // Yön buton renkleri
    private static readonly Color ColDirNormal   = new Color(0.25f, 0.40f, 0.65f);
    private static readonly Color ColDirSelected = new Color(0.18f, 0.70f, 0.92f);

    // ── State ─────────────────────────────────────────────────────────────────

    private GridCellData          selectedCell;
    private string                spawnerDir   = "Up";
    private readonly List<string> spawnerQueue = new();

    private Dictionary<string, Button> dirButtons;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void OnEnable()
    {
        BindButtons();
        RefreshGrid();
    }

    private void OnDisable()
    {
        UnbindButtons();
    }

    // ── Buton bağlama ─────────────────────────────────────────────────────────

    private void BindButtons()
    {
        closeButton?.onClick.AddListener(OnCloseClicked);

        cpEmptyBtn?.onClick.AddListener(()    => OnContentSelected("Empty"));
        cpObstacleBtn?.onClick.AddListener(() => OnContentSelected("Obstacle"));
        cpPassengerBtn?.onClick.AddListener(() => OnContentSelected("Passenger"));
        cpSpawnerBtn?.onClick.AddListener(()  => OnContentSelected("Spawner"));
        cpCancelBtn?.onClick.AddListener(HideSubPanels);

        clrRedBtn?.onClick.AddListener(()    => OnColorSelected("Red"));
        clrBlueBtn?.onClick.AddListener(()   => OnColorSelected("Blue"));
        clrGreenBtn?.onClick.AddListener(()  => OnColorSelected("Green"));
        clrYellowBtn?.onClick.AddListener(() => OnColorSelected("Yellow"));
        clrCancelBtn?.onClick.AddListener(HideSubPanels);

        dirButtons = new Dictionary<string, Button>
        {
            { "Up",    spDirUp    },
            { "Down",  spDirDown  },
            { "Left",  spDirLeft  },
            { "Right", spDirRight },
        };
        spDirUp?.onClick.AddListener(()    => OnDirectionSelected("Up"));
        spDirDown?.onClick.AddListener(()  => OnDirectionSelected("Down"));
        spDirLeft?.onClick.AddListener(()  => OnDirectionSelected("Left"));
        spDirRight?.onClick.AddListener(() => OnDirectionSelected("Right"));

        spAddRed?.onClick.AddListener(()    => OnSpawnerColorAdd("Red"));
        spAddBlue?.onClick.AddListener(()   => OnSpawnerColorAdd("Blue"));
        spAddGreen?.onClick.AddListener(()  => OnSpawnerColorAdd("Green"));
        spAddYellow?.onClick.AddListener(() => OnSpawnerColorAdd("Yellow"));

        spDeleteBtn?.onClick.AddListener(OnSpawnerColorDelete);
        spConfirmBtn?.onClick.AddListener(OnSpawnerConfirm);
        spCancelBtn?.onClick.AddListener(HideSubPanels);
    }

    private void UnbindButtons()
    {
        closeButton?.onClick.RemoveAllListeners();

        cpEmptyBtn?.onClick.RemoveAllListeners();
        cpObstacleBtn?.onClick.RemoveAllListeners();
        cpPassengerBtn?.onClick.RemoveAllListeners();
        cpSpawnerBtn?.onClick.RemoveAllListeners();
        cpCancelBtn?.onClick.RemoveAllListeners();

        clrRedBtn?.onClick.RemoveAllListeners();
        clrBlueBtn?.onClick.RemoveAllListeners();
        clrGreenBtn?.onClick.RemoveAllListeners();
        clrYellowBtn?.onClick.RemoveAllListeners();
        clrCancelBtn?.onClick.RemoveAllListeners();

        spDirUp?.onClick.RemoveAllListeners();
        spDirDown?.onClick.RemoveAllListeners();
        spDirLeft?.onClick.RemoveAllListeners();
        spDirRight?.onClick.RemoveAllListeners();

        spAddRed?.onClick.RemoveAllListeners();
        spAddBlue?.onClick.RemoveAllListeners();
        spAddGreen?.onClick.RemoveAllListeners();
        spAddYellow?.onClick.RemoveAllListeners();

        spDeleteBtn?.onClick.RemoveAllListeners();
        spConfirmBtn?.onClick.RemoveAllListeners();
        spCancelBtn?.onClick.RemoveAllListeners();
    }

    // ── Grid ──────────────────────────────────────────────────────────────────

    private void RefreshGrid()
    {
        if (gridContainer == null) return;

        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        var level = editorPanel?.CurrentLevel;
        if (level == null) return;

        int   w    = level.gridWidth;
        int   h    = level.gridHeight;
        float gap  = 2f;

        // GridLayoutGroup'u bul ya da ekle
        var glg = gridContainer.GetComponent<GridLayoutGroup>()
               ?? gridContainer.gameObject.AddComponent<GridLayoutGroup>();

        // Hücre boyutunu container genişliğine göre hesapla
        var containerRt = gridContainer.GetComponent<RectTransform>();
        float availW = containerRt != null ? containerRt.rect.width  : 600f;
        float availH = containerRt != null ? containerRt.rect.height : 480f;
        float cellW  = (availW - gap * (w - 1) - 12f) / w;
        float cellH  = (availH - gap * (h - 1) - 12f) / h;
        float cell   = Mathf.Min(cellW, cellH, 140f);

        glg.cellSize        = new Vector2(cell, cell);
        glg.spacing         = new Vector2(gap, gap);
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = w;
        glg.childAlignment  = TextAnchor.MiddleCenter;
        glg.padding         = new RectOffset(6, 6, 6, 6);

        foreach (var cellData in level.grid)
            CreateTileButton(cellData);
    }

    private void CreateTileButton(GridCellData cell)
    {
        var go = new GameObject($"Tile_{cell.x}_{cell.y}");
        go.transform.SetParent(gridContainer, false);
        go.AddComponent<RectTransform>();

        var img = go.AddComponent<Image>();
        img.color = TileColor(cell);

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = TileColor(cell);
        cb.highlightedColor = TileColor(cell) * 1.3f;
        cb.pressedColor     = TileColor(cell) * 0.7f;
        cb.colorMultiplier  = 1f;
        btn.colors = cb;

        // Etiket
        var lGo = new GameObject("Lbl");
        lGo.transform.SetParent(go.transform, false);
        var lRt = lGo.AddComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero;
        lRt.anchorMax = Vector2.one;
        lRt.offsetMin = new Vector2(2f, 2f);
        lRt.offsetMax = new Vector2(-2f, -2f);
        var tmp = lGo.AddComponent<TextMeshProUGUI>();
        tmp.text             = TileLabel(cell);
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.color            = Color.white;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin      = 10f;
        tmp.fontSizeMax      = 22f;

        var captured = cell;
        btn.onClick.AddListener(() => OnTileClicked(captured));
    }

    // ── Interaction ───────────────────────────────────────────────────────────

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnTileClicked(GridCellData cell)
    {
        selectedCell = cell;
        HideSubPanels();
        contentPickerPanel?.SetActive(true);
    }

    private void OnContentSelected(string type)
    {
        contentPickerPanel?.SetActive(false);
        switch (type)
        {
            case "Empty":
                Apply("Empty", "");
                break;
            case "Obstacle":
                Apply("Obstacle", "");
                break;
            case "Passenger":
                colorPickerPanel?.SetActive(true);
                break;
            case "Spawner":
                OpenSpawnerConfig();
                break;
        }
    }

    private void OnColorSelected(string color)
    {
        selectedCell.contentType = "Passenger";
        selectedCell.color       = color;
        selectedCell.direction   = "";
        selectedCell.spawnColors = null;
        HideSubPanels();
        RefreshGrid();
    }

    private void Apply(string type, string color)
    {
        selectedCell.contentType = type;
        selectedCell.color       = color;
        selectedCell.direction   = "";
        selectedCell.spawnColors = null;
        HideSubPanels();
        RefreshGrid();
    }

    // ── Spawner Config ────────────────────────────────────────────────────────

    private void OpenSpawnerConfig()
    {
        spawnerDir = string.IsNullOrEmpty(selectedCell.direction) ? "Up" : selectedCell.direction;
        spawnerQueue.Clear();
        if (selectedCell.spawnColors != null)
            spawnerQueue.AddRange(selectedCell.spawnColors);

        RefreshSpawnerUI();
        spawnerConfigPanel?.SetActive(true);
    }

    private void OnDirectionSelected(string dir)
    {
        spawnerDir = dir;
        RefreshSpawnerUI();
    }

    private void OnSpawnerColorAdd(string color)
    {
        if (spawnerQueue.Count >= SpawnerMax) return;
        spawnerQueue.Add(color);
        RefreshSpawnerUI();
    }

    private void OnSpawnerColorDelete()
    {
        if (spawnerQueue.Count == 0) return;
        spawnerQueue.RemoveAt(spawnerQueue.Count - 1);
        RefreshSpawnerUI();
    }

    private void OnSpawnerConfirm()
    {
        if (spawnerQueue.Count < SpawnerMin)
        {
            Debug.LogWarning("[TileEditorPanel] En az 1 renk gerekli!");
            return;
        }
        selectedCell.contentType = "Spawner";
        selectedCell.direction   = spawnerDir;
        selectedCell.spawnColors = spawnerQueue.ToArray();
        selectedCell.color       = "";
        HideSubPanels();
        RefreshGrid();
    }

    private void RefreshSpawnerUI()
    {
        if (spDirectionLabel != null)
            spDirectionLabel.text = $"Dir=: {spawnerDir}";

        if (spQueueLabel != null)
        {
            spQueueLabel.text = spawnerQueue.Count == 0
                ? "Kuyruk: (bos) - en az 1 renk ekleyin"
                : "Kuyruk: " + string.Join(", ", spawnerQueue);
        }

        if (dirButtons == null) return;
        foreach (var kvp in dirButtons)
        {
            if (kvp.Value == null) continue;
            var cb = kvp.Value.colors;
            bool sel = kvp.Key == spawnerDir;
            cb.normalColor      = sel ? ColDirSelected : ColDirNormal;
            cb.highlightedColor = cb.normalColor * 1.2f;
            cb.pressedColor     = cb.normalColor * 0.75f;
            cb.colorMultiplier  = 1f;
            kvp.Value.colors = cb;
        }
    }

    // ── Yardımcılar ───────────────────────────────────────────────────────────

    private void HideSubPanels()
    {
        contentPickerPanel?.SetActive(false);
        colorPickerPanel?.SetActive(false);
        spawnerConfigPanel?.SetActive(false);
    }

    private static Color TileColor(GridCellData cell)
    {
        switch (cell.contentType)
        {
            case "Obstacle":  return ColObstacle;
            case "Spawner":   return ColSpawner;
            case "Passenger":
                return ColorMap.TryGetValue(cell.color, out var c) ? c : ColEmpty;
            default:          return ColEmpty;
        }
    }

    private static string TileLabel(GridCellData cell)
    {
        switch (cell.contentType)
        {
            case "Obstacle":  return "Obstacle";
            case "Passenger": return $"Pasenger\n{cell.color}";
            case "Spawner":
                int cnt = cell.spawnColors?.Length ?? 0;
                return $"SPAWN\n{cell.direction}\n({cnt})";
            default:          return "Empty";
        }
    }
}
