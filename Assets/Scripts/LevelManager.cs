using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private TextAsset levelJson;

    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private BusStation busStation;

    private LevelData currentLevelData;

    private void Start()
    {
        StartCoroutine(LoadLevel());
    }

    public IEnumerator LoadLevel()
    {
        if (levelJson == null)
        {
            Debug.LogError("Level json atanmadı");
            yield break;
        }

        currentLevelData = JsonUtility.FromJson<LevelData>(levelJson.text);

        if (currentLevelData == null)
        {
            Debug.LogError("Level json parse edilemedi");
            yield break;
        }

        if (gridManager == null)
        {
            Debug.LogError("GridManager atanmadı");
            yield break;
        }

        if (waitingAreaManager == null)
        {
            Debug.LogError("WaitingAreaManager atanmadı");
            yield break;
        }

        if (busStation == null)
        {
            Debug.LogError("BusStation atanmadı");
            yield break;
        }

        gridManager.CreateGrid(currentLevelData.gridWidth, currentLevelData.gridHeight);
        waitingAreaManager.CreateWaitingArea(currentLevelData.waitingAreaLength);

        yield return StartCoroutine(busStation.InitializeRoutine(currentLevelData.buses));

    }
}