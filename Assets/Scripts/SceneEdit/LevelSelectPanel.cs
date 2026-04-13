using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editör sahnesindeki level seçim panelini yönetir.
/// Assets/Levels/ altındaki tüm JSON'ları listeler.
/// Seçim yapıldığında bu panel kapanır, editor paneli açılır.
/// </summary>
public class LevelSelectPanel : MonoBehaviour
{
    [Header("Liste")]
    [SerializeField] private Transform listContent;          // ScrollView > Viewport > Content
    [SerializeField] private LevelItemUI levelItemPrefab;    // Her level satırı için prefab

    [Header("Butonlar")]
    [SerializeField] private Button newLevelButton;

    [Header("Panel Geçişi")]
    [SerializeField] private GameObject editorPanel;         // Seçim sonrası açılacak panel

    // ── Seçili level bilgisi (statik — diğer paneller okuyabilir) ────────────
    public static string SelectedLevelPath { get; private set; }
    public static LevelData SelectedLevel  { get; private set; }
    public static bool IsNewLevel          { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (newLevelButton != null)
            newLevelButton.onClick.AddListener(OnNewLevelClicked);

        PopulateList();
    }

    // ── JSON'ları oku, listeyi doldur ────────────────────────────────────────

    private void PopulateList()
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        string levelsFolder = Path.Combine(Application.dataPath, "Levels");

        if (!Directory.Exists(levelsFolder))
        {
            Debug.LogWarning("[LevelSelectPanel] Assets/Levels/ klasörü bulunamadı.");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(levelsFolder, "*.json")
                                      .OrderBy(p => p)
                                      .ToArray();

        foreach (string filePath in jsonFiles)
        {
            string json = File.ReadAllText(filePath);
            LevelData data = JsonUtility.FromJson<LevelData>(json);
            if (data == null) continue;

            string assetPath = "Assets/Levels/" + Path.GetFileName(filePath);

            LevelItemUI item = Instantiate(levelItemPrefab, listContent);
            item.Initialize(data, assetPath, OnLevelSelected);
        }
    }

    // ── Seçim callback'leri ──────────────────────────────────────────────────

    private void OnLevelSelected(string assetPath, LevelData data)
    {
        SelectedLevelPath = assetPath;
        SelectedLevel     = data;
        IsNewLevel        = false;

        OpenEditorPanel();
    }

    private void OnNewLevelClicked()
    {
        SelectedLevelPath = null;
        SelectedLevel     = null;
        IsNewLevel        = true;

        OpenEditorPanel();
    }

    private void OpenEditorPanel()
    {
        gameObject.SetActive(false);

        if (editorPanel != null)
            editorPanel.SetActive(true);
    }

    // ── Editor panelinden geri dönüş ─────────────────────────────────────────

    public void ReturnToList()
    {
        if (editorPanel != null)
            editorPanel.SetActive(false);

        gameObject.SetActive(true);
        PopulateList();
    }
}
