using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grid tile editörü — LevelEditorPanel'daki "Edit Tiles" butonuyla açılır.
/// W×H'lik bir hücre ızgarası gösterir; hücreye tıklayınca içerik seçme
/// (Empty / Obstacle / Passenger / Spawner) popup'ı açılır.
///   • Passenger → renk seçme penceresi
///   • Spawner   → yön + renk kuyruğu (min 1 / maks 3)
/// Tüm UI Awake'te programatik olarak oluşturulur — inspector'da sadece
/// editorPanel referansı atanır.
/// </summary>
public class TileEditorPanel : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [SerializeField] private LevelEditorPanel editorPanel;

    // ── Sabitler ──────────────────────────────────────────────────────────────

    private const int SpawnerMin = 1;
    private const int SpawnerMax = 3;

    // ── Renk haritası ─────────────────────────────────────────────────────────

    private static readonly Dictionary<string, Color> ColorMap = new()
    {
        { "Red",    new Color(0.90f, 0.22f, 0.22f) },
        { "Blue",   new Color(0.22f, 0.45f, 0.90f) },
        { "Green",  new Color(0.20f, 0.78f, 0.32f) },
        { "Yellow", new Color(1.00f, 0.88f, 0.10f) },
    };

    private static readonly Color ColTileEmpty    = new Color(0.78f, 0.78f, 0.78f);
    private static readonly Color ColTileObstacle = new Color(0.38f, 0.38f, 0.38f);
    private static readonly Color ColTileSpawner  = new Color(1.00f, 0.55f, 0.00f);
    private static readonly Color ColPanel        = new Color(0.24f, 0.24f, 0.30f, 0.98f);
    private static readonly Color ColPopup        = new Color(0.30f, 0.30f, 0.38f, 1.00f);

    // ── State ─────────────────────────────────────────────────────────────────

    private GridCellData         selectedCell;
    private string               spawnerDirection = "Up";
    private readonly List<string> spawnerQueue    = new();

    // ── Runtime UI referansları ───────────────────────────────────────────────

    private Transform  gridContainer;
    private GameObject contentPickerGo;
    private GameObject colorPickerGo;
    private GameObject spawnerConfigGo;
    private TMP_Text   spawnerDirLabel;
    private TMP_Text   spawnerQueueLabel;

    private readonly Dictionary<string, Button> dirBtns = new();

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()  => BuildUI();
    private void OnEnable() => RefreshGrid();

    // ═════════════════════════════════════════════════════════════════════════
    // UI BUILDER
    // ═════════════════════════════════════════════════════════════════════════

    private void BuildUI()
    {
        // Panel root — tam ekran koyu overlay
        var rootRt = GetComponent<RectTransform>();
        if (rootRt != null)
        {
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
        }
        var rootImg = gameObject.AddComponent<Image>();
        rootImg.color = new Color(0f, 0f, 0f, 0.55f);

        // Ana panel kutusu
        var main = MakePanel(transform, "MainPanel",
            new Vector2(760f, 620f), Vector2.zero, ColPanel);

        // Başlık
        MakeLabel(main, "Title", "Tile Düzenle",
            new Vector2(-25f, 270f), new Vector2(620f, 42f), 22f);

        // Kapat butonu
        var closeBtn = MakeButton(main, "CloseBtn", "X",
            new Vector2(340f, 270f), new Vector2(44f, 44f),
            new Color(0.75f, 0.18f, 0.18f));
        closeBtn.onClick.AddListener(() => gameObject.SetActive(false));

        // Grid alanı
        BuildGridArea(main);

        // ── Alt paneller (root transform'a ekliyoruz: üstte render edilsin)
        BuildContentPicker(transform);
        BuildColorPicker(transform);
        BuildSpawnerConfig(transform);
    }

    // ── Grid container (basit, scroll yok) ───────────────────────────────────

    private void BuildGridArea(Transform parent)
    {
        var areaGo = new GameObject("GridArea");
        areaGo.transform.SetParent(parent, false);

        var areaRt = areaGo.AddComponent<RectTransform>();
        areaRt.anchoredPosition = new Vector2(0f, -26f);
        areaRt.sizeDelta        = new Vector2(700f, 510f);
        areaGo.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.16f, 1f);

        gridContainer = areaGo.transform;
    }

    // ── Content Picker (Empty / Obstacle / Passenger / Spawner) ──────────────

    private void BuildContentPicker(Transform parent)
    {
        var rt = MakePanel(parent, "ContentPicker",
            new Vector2(400f, 300f), Vector2.zero, ColPopup);
        contentPickerGo = rt.gameObject;

        MakeLabel(contentPickerGo.transform, "Title", "İçerik Seç",
            new Vector2(0f, 115f), new Vector2(360f, 36f), 20f);

        var types  = new[] { "Empty",   "Obstacle", "Passenger",                   "Spawner"    };
        var labels = new[] { "Boş",     "Engel",    "Yolcu",                       "Spawner"    };
        var colors = new[] { ColTileEmpty, ColTileObstacle,
                             new Color(0.90f, 0.22f, 0.22f), ColTileSpawner };

        float startX = -145f, spacing = 97f;
        for (int i = 0; i < types.Length; i++)
        {
            string t = types[i];
            var btn = MakeButton(contentPickerGo.transform, $"CP_{t}", labels[i],
                new Vector2(startX + i * spacing, 25f),
                new Vector2(85f, 85f), colors[i]);
            btn.onClick.AddListener(() => OnContentSelected(t));
        }

        var cancel = MakeButton(contentPickerGo.transform, "CP_Cancel", "İptal",
            new Vector2(0f, -100f), new Vector2(130f, 42f),
            new Color(0.48f, 0.48f, 0.48f));
        cancel.onClick.AddListener(HideSubPanels);

        contentPickerGo.SetActive(false);
    }

    // ── Color Picker ─────────────────────────────────────────────────────────

    private void BuildColorPicker(Transform parent)
    {
        var rt = MakePanel(parent, "ColorPicker",
            new Vector2(380f, 240f), Vector2.zero, ColPopup);
        colorPickerGo = rt.gameObject;

        MakeLabel(colorPickerGo.transform, "Title", "Renk Seç",
            new Vector2(0f, 90f), new Vector2(340f, 36f), 20f);

        var colors = new[] { "Red", "Blue", "Green", "Yellow" };
        float startX = -135f, spacing = 90f;
        for (int i = 0; i < colors.Length; i++)
        {
            string c = colors[i];
            var btn = MakeButton(colorPickerGo.transform, $"Clr_{c}", "",
                new Vector2(startX + i * spacing, 15f),
                new Vector2(64f, 64f), ColorMap[c]);
            btn.onClick.AddListener(() => OnColorSelected(c));
        }

        var cancel = MakeButton(colorPickerGo.transform, "Clr_Cancel", "İptal",
            new Vector2(0f, -78f), new Vector2(130f, 42f),
            new Color(0.48f, 0.48f, 0.48f));
        cancel.onClick.AddListener(HideSubPanels);

        colorPickerGo.SetActive(false);
    }

    // ── Spawner Config ────────────────────────────────────────────────────────

    private void BuildSpawnerConfig(Transform parent)
    {
        var rt = MakePanel(parent, "SpawnerConfig",
            new Vector2(460f, 440f), Vector2.zero, ColPopup);
        spawnerConfigGo = rt.gameObject;

        MakeLabel(spawnerConfigGo.transform, "Title", "Spawner Ayarla",
            new Vector2(0f, 190f), new Vector2(420f, 36f), 20f);

        // Yön başlığı
        spawnerDirLabel = MakeLabel(spawnerConfigGo.transform, "DirLabel", "Yön: Up",
            new Vector2(0f, 145f), new Vector2(420f, 28f), 15f);

        // Yön butonları
        var dirs      = new[] { "Up",      "Down",    "Left",  "Right" };
        var dirLabels = new[] { "^ Yukari","v Asagi", "< Sol", "> Sag" };
        float dX = -153f, dSpacing = 102f;
        for (int i = 0; i < dirs.Length; i++)
        {
            string d = dirs[i];
            var btn = MakeButton(spawnerConfigGo.transform, $"Dir_{d}", dirLabels[i],
                new Vector2(dX + i * dSpacing, 100f),
                new Vector2(92f, 38f), new Color(0.25f, 0.40f, 0.65f));
            dirBtns[d] = btn;
            btn.onClick.AddListener(() => OnDirectionSelected(d));
        }

        // Kuyruk etiketi
        spawnerQueueLabel = MakeLabel(spawnerConfigGo.transform, "QueueLabel",
            "Kuyruk: (boş) — en az 1 ekleyin",
            new Vector2(0f, 50f), new Vector2(420f, 28f), 13f);

        // Renk ekleme başlığı
        MakeLabel(spawnerConfigGo.transform, "AddTitle", "Renk Ekle (maks 3):",
            new Vector2(0f, 10f), new Vector2(420f, 24f), 13f);

        // Renk ekleme butonları
        var cNames = new[] { "Red", "Blue", "Green", "Yellow" };
        float cX = -135f, cSpacing = 90f;
        for (int i = 0; i < cNames.Length; i++)
        {
            string c = cNames[i];
            var btn = MakeButton(spawnerConfigGo.transform, $"SpAdd_{c}", "",
                new Vector2(cX + i * cSpacing, -40f),
                new Vector2(54f, 54f), ColorMap[c]);
            btn.onClick.AddListener(() => OnSpawnerColorAdd(c));
        }

        // Sil butonu
        var delBtn = MakeButton(spawnerConfigGo.transform, "Sp_Delete", "Sil",
            new Vector2(-80f, -110f), new Vector2(110f, 40f),
            new Color(0.70f, 0.20f, 0.20f));
        delBtn.onClick.AddListener(OnSpawnerColorDelete);

        // Onayla butonu
        var confirmBtn = MakeButton(spawnerConfigGo.transform, "Sp_Confirm", "Onayla",
            new Vector2(80f, -110f), new Vector2(110f, 40f),
            new Color(0.18f, 0.65f, 0.20f));
        confirmBtn.onClick.AddListener(OnSpawnerConfirm);

        // İptal butonu
        var cancelBtn = MakeButton(spawnerConfigGo.transform, "Sp_Cancel", "İptal",
            new Vector2(0f, -165f), new Vector2(130f, 40f),
            new Color(0.48f, 0.48f, 0.48f));
        cancelBtn.onClick.AddListener(HideSubPanels);

        spawnerConfigGo.SetActive(false);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // GRID REFRESH
    // ═════════════════════════════════════════════════════════════════════════

    private void RefreshGrid()
    {
        if (gridContainer == null) return;

        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        var level = editorPanel?.CurrentLevel;
        if (level == null) return;

        int   w       = level.gridWidth;
        int   h       = level.gridHeight;
        float gap     = 6f;
        float maxCell = 88f;
        float areaW   = 688f;
        float areaH   = 498f;
        float cellW   = (areaW - gap * (w - 1) - 12f) / w;
        float cellH   = (areaH - gap * (h - 1) - 12f) / h;
        float cell    = Mathf.Min(cellW, cellH, maxCell);

        var glg = gridContainer.GetComponent<GridLayoutGroup>()
               ?? gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(cell, cell);
        glg.spacing         = new Vector2(gap, gap);
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = w;
        glg.childAlignment  = TextAnchor.MiddleCenter;
        glg.padding         = new RectOffset(6, 6, 6, 6);

        foreach (var cellData in level.grid)
            CreateTileButton(cellData);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gridContainer);
    }

    private void CreateTileButton(GridCellData cell)
    {
        var go = new GameObject($"Tile_{cell.x}_{cell.y}");
        go.transform.SetParent(gridContainer, false);
        go.AddComponent<RectTransform>();

        var img = go.AddComponent<Image>();
        img.color = GetTileColor(cell);

        var btn  = go.AddComponent<Button>();
        var cb   = btn.colors;
        var tc   = GetTileColor(cell);
        cb.normalColor      = tc;
        cb.highlightedColor = tc * 1.28f;
        cb.pressedColor     = tc * 0.70f;
        cb.selectedColor    = tc;
        cb.colorMultiplier  = 1f;
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;

        // Hücre etiketi
        var txtGo = new GameObject("Lbl");
        txtGo.transform.SetParent(go.transform, false);
        var trt = txtGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(2f, 2f);
        trt.offsetMax = new Vector2(-2f, -2f);
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text             = GetTileLabel(cell);
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.color            = Color.white;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin      = 7f;
        tmp.fontSizeMax      = 15f;

        var captured = cell;
        btn.onClick.AddListener(() => OnTileClicked(captured));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // INTERACTION FLOW
    // ═════════════════════════════════════════════════════════════════════════

    private void OnTileClicked(GridCellData cell)
    {
        selectedCell = cell;
        HideSubPanels();
        contentPickerGo.SetActive(true);
    }

    private void OnContentSelected(string type)
    {
        contentPickerGo.SetActive(false);

        switch (type)
        {
            case "Empty":
                ApplySimple("Empty", "");
                break;

            case "Obstacle":
                ApplySimple("Obstacle", "");
                break;

            case "Passenger":
                colorPickerGo.SetActive(true);
                break;

            case "Spawner":
                OpenSpawnerConfig();
                break;
        }
    }

    private void OnColorSelected(string colorName)
    {
        selectedCell.contentType = "Passenger";
        selectedCell.color       = colorName;
        selectedCell.direction   = "";
        selectedCell.spawnColors = null;
        HideSubPanels();
        RefreshGrid();
    }

    private void ApplySimple(string type, string color)
    {
        selectedCell.contentType = type;
        selectedCell.color       = color;
        selectedCell.direction   = "";
        selectedCell.spawnColors = null;
        HideSubPanels();
        RefreshGrid();
    }

    // ── Spawner Config Flow ───────────────────────────────────────────────────

    private void OpenSpawnerConfig()
    {
        spawnerDirection = string.IsNullOrEmpty(selectedCell.direction) ? "Up" : selectedCell.direction;
        spawnerQueue.Clear();
        if (selectedCell.spawnColors != null)
            spawnerQueue.AddRange(selectedCell.spawnColors);

        RefreshSpawnerUI();
        spawnerConfigGo.SetActive(true);
    }

    private void OnDirectionSelected(string dir)
    {
        spawnerDirection = dir;
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
            Debug.LogWarning("[TileEditorPanel] Spawner için en az 1 renk gerekli!");
            return;
        }
        selectedCell.contentType = "Spawner";
        selectedCell.direction   = spawnerDirection;
        selectedCell.spawnColors = spawnerQueue.ToArray();
        selectedCell.color       = "";
        HideSubPanels();
        RefreshGrid();
    }

    private void RefreshSpawnerUI()
    {
        if (spawnerDirLabel != null)
            spawnerDirLabel.text = $"Yön: {spawnerDirection}";

        if (spawnerQueueLabel != null)
        {
            spawnerQueueLabel.text = spawnerQueue.Count == 0
                ? "Kuyruk: (boş) — en az 1 renk ekleyin"
                : "Kuyruk: " + string.Join(", ", spawnerQueue);
        }

        // Seçili yön butonu vurgulanır
        foreach (var kvp in dirBtns)
        {
            var cb = kvp.Value.colors;
            bool selected = kvp.Key == spawnerDirection;
            cb.normalColor      = selected
                ? new Color(0.18f, 0.70f, 0.92f)
                : new Color(0.25f, 0.40f, 0.65f);
            cb.highlightedColor = cb.normalColor * 1.20f;
            cb.pressedColor     = cb.normalColor * 0.75f;
            cb.colorMultiplier  = 1f;
            kvp.Value.colors = cb;
        }
    }

    // ── Yardımcılar ───────────────────────────────────────────────────────────

    private void HideSubPanels()
    {
        if (contentPickerGo) contentPickerGo.SetActive(false);
        if (colorPickerGo)   colorPickerGo.SetActive(false);
        if (spawnerConfigGo) spawnerConfigGo.SetActive(false);
    }

    private static Color GetTileColor(GridCellData cell)
    {
        switch (cell.contentType)
        {
            case "Obstacle":  return ColTileObstacle;
            case "Spawner":   return ColTileSpawner;
            case "Passenger":
                return ColorMap.TryGetValue(cell.color, out var c) ? c : ColTileEmpty;
            default:          return ColTileEmpty;
        }
    }

    private static string GetTileLabel(GridCellData cell)
    {
        switch (cell.contentType)
        {
            case "Obstacle":  return "ENGEL";
            case "Passenger": return $"YOLCU\n{cell.color}";
            case "Spawner":
                int cnt = cell.spawnColors?.Length ?? 0;
                return $"SPAWN\n{cell.direction}\n({cnt})";
            default:          return "BOŞ";
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // UI FACTORY
    // ═════════════════════════════════════════════════════════════════════════

    private static RectTransform MakePanel(Transform parent, string name,
        Vector2 size, Vector2 pos, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = bgColor;
        return rt;
    }

    private static Button MakeButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = bgColor;

        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor * 1.25f;
        cb.pressedColor     = bgColor * 0.72f;
        cb.selectedColor    = bgColor;
        cb.colorMultiplier  = 1f;
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;

        if (!string.IsNullOrEmpty(label))
        {
            var tGo = new GameObject("Txt");
            tGo.transform.SetParent(go.transform, false);
            var trt = tGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(3f, 3f);
            trt.offsetMax = new Vector2(-3f, -3f);
            var tmp = tGo.AddComponent<TextMeshProUGUI>();
            tmp.text             = label;
            tmp.alignment        = TextAlignmentOptions.Center;
            tmp.color            = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin      = 8f;
            tmp.fontSizeMax      = 18f;
        }

        return btn;
    }

    private static TMP_Text MakeLabel(Transform parent, string name, string text,
        Vector2 pos, Vector2 size, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        return tmp;
    }
}
