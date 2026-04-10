using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Data")]
    [SerializeField] private TextAsset levelJson;

    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private BusStation busStation;

    private LevelData currentLevelData;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PassengerContent passenger = hit.collider.GetComponentInParent<PassengerContent>();
                if (passenger == null)
                {
                    Tile tile = hit.collider.GetComponentInParent<Tile>();
                    if (tile != null && tile.GetContent() is PassengerContent)
                    {
                        passenger = (PassengerContent)tile.GetContent();
                    }
                }

                if (passenger != null)
                {
                    OnPassengerClicked(passenger);
                }
            }
        }
    }

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

        if (gridManager == null || waitingAreaManager == null || busStation == null)
        {
            Debug.LogError("Bağlantılar eksik");
            yield break;
        }

        gridManager.CreateGrid(currentLevelData.gridWidth, currentLevelData.gridHeight, currentLevelData.grid);
        waitingAreaManager.CreateWaitingArea(currentLevelData.waitingAreaLength);

        yield return StartCoroutine(busStation.InitializeRoutine(currentLevelData.buses));
        
        OnImportantActionComplete();
    }

    public void OnPassengerClicked(PassengerContent passenger)
    {
        if (isGameOver || !busStation.IsReady()) return;

        if (waitingAreaManager.IsFull()) return;

        Tile currTile = passenger.GetOwnerTile();
        if (currTile == null) return; 

        System.Collections.Generic.List<Vector3> path = gridManager.GetPathToRoad(currTile.GetX(), currTile.GetY());
        if (path == null)
            return;

        Bus currentBus = busStation.GetCurrentBus();
        bool canBoardBus = false;

        if (currentBus != null && !currentBus.IsFullyReserved() && currentBus.GetBusColor() == passenger.GetColor())
        {
            canBoardBus = true;
        }

        currTile.ClearContent();
        passenger.SetOwnerTile(null); 

        if (canBoardBus)
        {
            currentBus.ReserveSeat();
            
            passenger.MoveAlongPath(path, currentBus.transform.position, 10f, () => {
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
                targetWaitingTile.SetContent(passenger);
                
                passenger.MoveAlongPath(path, targetWaitingTile.transform.position, 10f, () => {
                    if (waitingAreaManager.IsFull())
                    {
                        isGameOver = true;
                        Debug.Log("Oyun bitti, Waiting Area Doldu!");
                    }
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
                        
                        p.MoveTo(currentBus.transform.position, 10f, () => {
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
    }
}