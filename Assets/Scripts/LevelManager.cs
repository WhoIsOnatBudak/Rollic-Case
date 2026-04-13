using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Data")]
    [SerializeField] private TextAsset[] levelJsons;

    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private BusStation busStation;
    [SerializeField] private LevelUIController levelUIController;

    private LevelData currentLevelData;
    private bool isGameOver = false;

    [HideInInspector] public int activeMovements = 0;

    public bool IsGameOver => isGameOver;
    public bool IsBusStationReady() => busStation?.IsReady() ?? false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(LoadLevel());
    }

    public IEnumerator LoadLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (levelJsons == null || levelJsons.Length == 0)
        {
            Debug.LogError("Level jsons array is empty");
            yield break;
        }

        int levelIndex = currentLevel - 1;
        if (levelIndex >= levelJsons.Length)
        {
            levelIndex = levelJsons.Length - 1; // repeat last level if out of bounds
        }

        TextAsset levelJson = levelJsons[levelIndex];

        if (levelJson == null)
        {
            Debug.LogError("Level json is null at index " + levelIndex);
            yield break;
        }

        currentLevelData = JsonUtility.FromJson<LevelData>(levelJson.text);

        if (currentLevelData == null)
        {
            Debug.LogError("Level json parse edilemedi");
            yield break;
        }

        if (gridManager == null || waitingAreaManager == null || busStation == null)
        {
            Debug.LogError("Bağlantılar eksik");
            yield break;
        }

        if (levelUIController != null)
        {
            levelUIController.Initialize(currentLevelData.levelNumber, currentLevelData.timerSeconds);
        }

        gridManager.CreateGrid(currentLevelData.gridWidth, currentLevelData.gridHeight, currentLevelData.grid);
        waitingAreaManager.CreateWaitingArea(currentLevelData.waitingAreaLength);

        yield return StartCoroutine(busStation.InitializeRoutine(currentLevelData.buses));

        if (levelUIController != null)
            levelUIController.SetPowerUpsEnabled(true);

        OnImportantActionComplete();
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (levelUIController != null)
        {
            levelUIController.StopTimer();
            levelUIController.ShowGameOver();
        }

        Debug.Log("Oyun Bitti! Kaybedildi.");
    }

    public void LevelComplete()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (levelUIController != null)
        {
            levelUIController.StopTimer();
            levelUIController.ShowLevelComplete();
        }

        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();

        Debug.Log("Bolum Tamamlandi! Kazanildi.");
    }

    public void CheckWinCondition()
    {
        if (isGameOver) return;
        if (activeMovements > 0) return;

        // Check if any passenger or spawner is on the grid
        Tile[,] gridTiles = gridManager.GetGridTiles();
        if (gridTiles != null)
        {
            foreach (Tile t in gridTiles)
            {
                if (t != null && !t.IsEmpty())
                {
                    if (t.GetContent() is PassengerContent)
                    {
                        return; // a passenger still on grid
                    }
                    if (t.GetContent() is SpawnerContent spawner && spawner.HasPassengers())
                    {
                        return; // spawner still has passengers
                    }
                }
            }
        }

        // Check if waiting area has passengers
        foreach (Tile wt in waitingAreaManager.GetWaitingAreaTiles())
        {
            if (!wt.IsEmpty()) return;
        }

        // All empty and no movements? Level Complete!
        LevelComplete();
    }

    public void CheckLoseCondition()
    {
        if (isGameOver) return;
        if (activeMovements > 0) return;
        if (!busStation.IsReady()) return;

        if (waitingAreaManager.IsFull())
        {
            GameOver();
        }
    }

    public void OnPassengerClicked(PassengerContent passenger)
    {
        if (levelUIController != null)
            levelUIController.StartTimerIfNeeded();

        if (isGameOver || !busStation.IsReady()) return;

        if (waitingAreaManager.IsFull()) return;

        Tile currTile = passenger.GetOwnerTile();
        if (currTile == null) return; 

        System.Collections.Generic.List<Vector3> path = gridManager.GetPathToRoad(currTile.GetX(), currTile.GetY());
        if (path == null)
        {
            passenger.PlayNegativeFeedback();
            return;
        }

        Bus currentBus = busStation.GetCurrentBus();
        bool canBoardBus = false;

        if (currentBus != null && !currentBus.IsFullyReserved() && currentBus.GetBusColor() == passenger.GetColor())
        {
            canBoardBus = true;
        }

        currTile.ClearContent();
        passenger.SetOwnerTile(null); 

        SpawnerContent.CheckAndTriggerAdjacentSpawners(currTile, gridManager);

        if (canBoardBus)
        {
            currentBus.ReserveSeat();
            
            activeMovements++;
            passenger.MoveAlongPath(path, currentBus.transform.position, 10f, () => {
                activeMovements--;
                currentBus.AddPassenger(passenger.gameObject);
                if (currentBus.IsFull())
                {
                    busStation.OnCurrentBusFull();
                }
                OnImportantActionComplete();
            });
        }
        else
        {
            Tile targetWaitingTile = waitingAreaManager.GetFirstEmptyTile();
            if (targetWaitingTile != null)
            {
                PowerUpManager.Instance?.PushUndo(passenger, currTile, targetWaitingTile);

                targetWaitingTile.SetContent(passenger);

                activeMovements++;
                passenger.MoveAlongPath(path, targetWaitingTile.transform.position, 10f, () => {
                    activeMovements--;
                    OnImportantActionComplete();
                });
            }
        }
    }

    public void OnImportantActionComplete()
    {
        if (isGameOver || !busStation.IsReady()) return;

        Bus currentBus = busStation.GetCurrentBus();
        if (currentBus == null) return;

        foreach(Tile t in waitingAreaManager.GetWaitingAreaTiles())
        {
            if (!t.IsEmpty())
            {
                PassengerContent p = t.GetContent() as PassengerContent;
                if (p != null)
                {
                    if (!currentBus.IsFullyReserved() && currentBus.GetBusColor() == p.GetColor())
                    {
                        t.ClearContent();
                        currentBus.ReserveSeat();
                        
                        activeMovements++;
                        p.MoveTo(currentBus.transform.position, 10f, () => {
                            activeMovements--;
                            currentBus.AddPassenger(p.gameObject);
                            if (currentBus.IsFull())
                            {
                                busStation.OnCurrentBusFull();
                            }
                            OnImportantActionComplete();
                        });
                    }
                }
            }
        }

        CheckWinCondition();
        CheckLoseCondition();
    }
}
