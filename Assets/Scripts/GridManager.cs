using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Transform contentParent;
    [SerializeField] private float tileSpacing = 1.2f;

    [Header("Content Prefabs")]
    [SerializeField] private PassengerContent passengerPrefab;
    [SerializeField] private ObstacleContent obstaclePrefab;

    private Tile[,] gridTiles;

    public void CreateGrid(int width, int height, GridCellData[] gridData)
    {
        ClearGrid();

        gridTiles = new Tile[width, height];

        float startX = -(width - 1) * tileSpacing / 2f;
        float startZ = 0f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile newTile = Instantiate(tilePrefab, gridParent);

                Vector3 localPosition = new Vector3(
                    startX + x * tileSpacing,
                    0f,
                    startZ - y * tileSpacing
                );

                newTile.transform.localPosition = localPosition;
                newTile.Initialize(x, y);

                gridTiles[x, y] = newTile;
            }
        }

        InitializeContents(gridData);
    }

    private void InitializeContents(GridCellData[] gridData)
    {
        if (gridData == null)
            return;

        for (int i = 0; i < gridData.Length; i++)
        {
            GridCellData cellData = gridData[i];
            Tile tile = GetTile(cellData.x, cellData.y);

            if (tile == null)
                continue;

            CreateContentForTile(tile, cellData);
        }
    }

    private void CreateContentForTile(Tile tile, GridCellData cellData)
    {
        if (cellData.contentType == "Passenger")
        {
            PassengerContent passenger = Instantiate(
                passengerPrefab,
                tile.transform.position,
                Quaternion.identity,
                contentParent
            );

            passenger.SetColor(ParseColorType(cellData.color));
            passenger.Initialize(tile);
            tile.SetContent(passenger);
        }
        else if (cellData.contentType == "Obstacle")
        {
            ObstacleContent obstacle = Instantiate(
                obstaclePrefab,
                tile.transform.position,
                Quaternion.identity,
                contentParent
            );

            obstacle.Initialize(tile);
            tile.SetContent(obstacle);
        }
        else
        {
            tile.ClearContent();
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (gridTiles == null)
            return null;

        if (x < 0 || y < 0 || x >= gridTiles.GetLength(0) || y >= gridTiles.GetLength(1))
            return null;

        return gridTiles[x, y];
    }

    public Tile[,] GetGridTiles()
    {
        return gridTiles;
    }

    private ColorType ParseColorType(string colorString)
    {
        switch (colorString)
        {
            case "Red":
                return ColorType.Red;
            case "Blue":
                return ColorType.Blue;
            case "Green":
                return ColorType.Green;
            case "Yellow":
                return ColorType.Yellow;
            default:
                Debug.LogWarning("Unknown color: " + colorString + ", defaulting to Red");
                return ColorType.Red;
        }
    }

    public bool CanReachRoad(int startX, int startY)
    {
        if (startY == 0) return true;

        if (gridTiles == null) return false;

        int width = gridTiles.GetLength(0);
        int height = gridTiles.GetLength(1);

        bool[,] visited = new bool[width, height];
        System.Collections.Generic.Queue<Vector2Int> queue = new System.Collections.Generic.Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        Vector2Int[] directions = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current.y == 0) return true;

            foreach (Vector2Int dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (!visited[nx, ny])
                    {
                        Tile tile = gridTiles[nx, ny];
                        // If tile is Walkable it means it is empty, so we can step on it/pass through it.
                        if (tile != null && tile.IsWalkable())
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
        }
        return false;
    }

    private void ClearGrid()
    {
        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                Destroy(gridParent.GetChild(i).gameObject);
            }
        }

        if (contentParent != null)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
        }
    }
}