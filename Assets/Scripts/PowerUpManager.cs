using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private GridManager gridManager;

    public int ExtraTileUsesLeft { get; private set; }
    public int UndoUsesLeft { get; private set; }

    private struct UndoAction
    {
        public PassengerContent passenger;
        public Tile fromTile;
        public Tile toTile;
    }
    private Stack<UndoAction> undoStack = new Stack<UndoAction>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SyncPowerUpsWithInventory();
    }

    // --- Extra Tile ---

    public void ExecuteExtraTile()
    {
        LevelManager lm = LevelManager.Instance;
        if (lm == null || lm.IsGameOver || !lm.IsBusStationReady()) return;
        if (ExtraTileUsesLeft <= 0) return;
        if (!MarketManager.TryConsumePowerUp(MarketPowerUpType.ExtraTile)) return;

        SyncPowerUpsWithInventory();
        waitingAreaManager.AddExtraTile();
    }

    // --- Undo ---

    public void PushUndo(PassengerContent passenger, Tile fromTile, Tile toTile)
    {
        undoStack.Push(new UndoAction
        {
            passenger = passenger,
            fromTile = fromTile,
            toTile = toTile
        });
    }

    public void ClearUndoStack()
    {
        undoStack.Clear();
    }

    public void ExecuteUndo()
    {
        LevelManager lm = LevelManager.Instance;
        if (lm == null || lm.IsGameOver || !lm.IsBusStationReady()) return;
        if (UndoUsesLeft <= 0) return;
        if (lm.activeMovements > 0) return;

        // Geçerli undo action bul — yolcu hâlâ waiting area tile'ında mı?
        UndoAction? validAction = null;
        while (undoStack.Count > 0)
        {
            UndoAction candidate = undoStack.Pop();
            if (candidate.toTile.GetContent() == candidate.passenger)
            {
                validAction = candidate;
                break;
            }
            // Yolcu otobüse bindi ya da başka yere gitti, atla
        }

        if (validAction is null) return;
        if (!MarketManager.TryConsumePowerUp(MarketPowerUpType.Undo)) return;

        SyncPowerUpsWithInventory();
        UndoAction action = validAction.Value;

        // Waiting area tile'ını serbest bırak
        action.toTile.ClearContent();

        // Spawner conflict: fromTile'da spawner'ın koyduğu yolcu varsa geri al
        if (!action.fromTile.IsEmpty())
        {
            PassengerContent occupant = action.fromTile.GetContent() as PassengerContent;
            if (occupant != null)
            {
                SpawnerContent sourceSpawner = FindSpawnerFacingTile(action.fromTile);
                if (sourceSpawner != null)
                    sourceSpawner.ReturnPassenger(occupant.GetColor());

                action.fromTile.ClearContent();
                Destroy(occupant.gameObject);
            }
        }

        // Yolcuyu orijinal grid tile'ına geri taşı
        action.fromTile.SetContent(action.passenger);

        lm.activeMovements++;
        action.passenger.MoveTo(action.fromTile.transform.position, 10f, () => {
            lm.activeMovements--;
            lm.OnImportantActionComplete();
        });
    }

    private SpawnerContent FindSpawnerFacingTile(Tile tile)
    {
        int x = tile.GetX();
        int y = tile.GetY();

        SpawnerContent s;
        s = GetSpawnerAt(x, y + 1, TileDirection.Up);    if (s != null) return s;
        s = GetSpawnerAt(x, y - 1, TileDirection.Down);  if (s != null) return s;
        s = GetSpawnerAt(x + 1, y, TileDirection.Left);  if (s != null) return s;
        s = GetSpawnerAt(x - 1, y, TileDirection.Right); if (s != null) return s;
        return null;
    }

    private SpawnerContent GetSpawnerAt(int sx, int sy, TileDirection dir)
    {
        Tile t = gridManager.GetTile(sx, sy);
        if (t != null && t.GetContent() is SpawnerContent spawner && spawner.GetFacingDirection() == dir)
            return spawner;
        return null;
    }

    private void SyncPowerUpsWithInventory()
    {
        MarketManager.EnsureInitialized();
        ExtraTileUsesLeft = MarketManager.GetPowerUpCount(MarketPowerUpType.ExtraTile);
        UndoUsesLeft = MarketManager.GetPowerUpCount(MarketPowerUpType.Undo);
    }
}
