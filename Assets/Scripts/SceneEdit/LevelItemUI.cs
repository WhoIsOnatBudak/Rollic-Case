using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Level listesindeki her bir satır item'ının scripti.
/// LevelSelectPanel tarafından Instantiate edilir.
/// </summary>
public class LevelItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelNameText;

    private string _assetPath;
    private LevelData _data;
    private Action<string, LevelData> _onSelect;

    public void Initialize(LevelData data, string assetPath, Action<string, LevelData> onSelect)
    {
        _assetPath = assetPath;
        _data      = data;
        _onSelect  = onSelect;

        if (levelNameText != null)
            levelNameText.text = $"Level {data.levelNumber}";


        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClicked);
    }

    private void OnClicked() => _onSelect?.Invoke(_assetPath, _data);
}
