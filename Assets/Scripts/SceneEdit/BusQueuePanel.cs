using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EditorPanel içindeki bus queue editörü.
/// LevelEditorPanel.CurrentLevel.buses dizisini okur / yazar.
/// </summary>
public class BusQueuePanel : MonoBehaviour
{
    // ── Inspector referansları ────────────────────────────────────────────────

    [Header("Renk Butonları")]
    [SerializeField] private Button redButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button yellowButton;

    [Header("Kontrol")]
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;

    [Header("Queue Görünümü")]
    [SerializeField] private Transform queueContainer;   // HorizontalLayoutGroup altındaki Content

    [Header("Referans")]
    [SerializeField] private LevelEditorPanel editorPanel;

    // ── Limitler ─────────────────────────────────────────────────────────────

    private const int MaxBuses    = 18;
    private const int BusesPerRow =  9;

    // ── Renk haritası ─────────────────────────────────────────────────────────

    private static readonly Dictionary<string, Color> ColorMap = new()
    {
        { "Red",    new Color(0.90f, 0.22f, 0.22f) },
        { "Blue",   new Color(0.22f, 0.45f, 0.90f) },
        { "Green",  new Color(0.20f, 0.78f, 0.32f) },
        { "Yellow", new Color(1.00f, 0.88f, 0.10f) },
    };

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        SetupGridLayout();
    }

    private void OnEnable()
    {
        BindButtons();
        RefreshQueue();
    }

    private void OnDisable()
    {
        UnbindButtons();
    }

    // ── Buton bağlama ─────────────────────────────────────────────────────────

    private void BindButtons()
    {
        redButton?.onClick.AddListener(OnRedClicked);
        blueButton?.onClick.AddListener(OnBlueClicked);
        greenButton?.onClick.AddListener(OnGreenClicked);
        yellowButton?.onClick.AddListener(OnYellowClicked);
        deleteButton?.onClick.AddListener(OnDeleteClicked);
        closeButton?.onClick.AddListener(OnCloseClicked);
    }

    private void UnbindButtons()
    {
        redButton?.onClick.RemoveListener(OnRedClicked);
        blueButton?.onClick.RemoveListener(OnBlueClicked);
        greenButton?.onClick.RemoveListener(OnGreenClicked);
        yellowButton?.onClick.RemoveListener(OnYellowClicked);
        deleteButton?.onClick.RemoveListener(OnDeleteClicked);
        closeButton?.onClick.RemoveListener(OnCloseClicked);
    }

    // ── Buton callback'leri ───────────────────────────────────────────────────

    private void OnRedClicked()    => AddBus("Red");
    private void OnBlueClicked()   => AddBus("Blue");
    private void OnGreenClicked()  => AddBus("Green");
    private void OnYellowClicked() => AddBus("Yellow");

    private void OnDeleteClicked()
    {
        var level = editorPanel?.CurrentLevel;
        if (level == null || level.buses == null || level.buses.Length == 0) return;

        level.buses = level.buses.Take(level.buses.Length - 1).ToArray();
        RefreshQueue();
    }

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }

    // ── Bus ekle ──────────────────────────────────────────────────────────────

    private void AddBus(string colorName)
    {
        var level = editorPanel?.CurrentLevel;
        if (level == null) return;
        if (level.buses != null && level.buses.Length >= MaxBuses) return;

        var list = level.buses != null
            ? new List<BusData>(level.buses)
            : new List<BusData>();

        list.Add(new BusData { color = colorName });
        level.buses = list.ToArray();

        RefreshQueue();
    }

    // ── Grid layout kurulumu (9 kolon, otomatik wrap) ────────────────────────

    private void SetupGridLayout()
    {
        if (queueContainer == null) return;

        // Varsa HorizontalLayoutGroup'u kaldır, GridLayoutGroup ekle
        var hlg = queueContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) Destroy(hlg);

        var grid = queueContainer.GetComponent<GridLayoutGroup>();
        grid ??= queueContainer.gameObject.AddComponent<GridLayoutGroup>();

        grid.cellSize        = new Vector2(24f, 24f);
        grid.spacing         = new Vector2(6f, 6f);
        grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = BusesPerRow;
        grid.childAlignment  = TextAnchor.UpperLeft;
    }

    // ── Queue görünümünü yenile ───────────────────────────────────────────────

    private void RefreshQueue()
    {
        if (queueContainer == null) return;

        foreach (Transform child in queueContainer)
            Destroy(child.gameObject);

        var level = editorPanel?.CurrentLevel;
        if (level?.buses != null)
        {
            foreach (var bus in level.buses)
                CreateBusItem(bus.color);
        }

        int count = level?.buses?.Length ?? 0;

        // Delete sadece dizi doluyken aktif
        if (deleteButton != null)
            deleteButton.interactable = count > 0;

        // Renk butonları max'a ulaşınca pasif
        bool canAdd = count < MaxBuses;
        if (redButton)    redButton.interactable    = canAdd;
        if (blueButton)   blueButton.interactable   = canAdd;
        if (greenButton)  greenButton.interactable  = canAdd;
        if (yellowButton) yellowButton.interactable = canAdd;
    }

    // ── Queue item programmatic oluştur ──────────────────────────────────────

    private void CreateBusItem(string colorName)
    {
        var go = new GameObject($"BusItem_{colorName}");
        go.transform.SetParent(queueContainer, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(24f, 24f);

        var img = go.AddComponent<Image>();
        if (ColorMap.TryGetValue(colorName, out Color c))
            img.color = c;
    }
}
