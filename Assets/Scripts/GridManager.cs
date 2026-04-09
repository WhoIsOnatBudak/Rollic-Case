using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private float tileSpacing = 1.2f;

    private Tile[,] gridTiles;

    public void CreateGrid(int width, int height)
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

                newTile.transform.localPosition = new Vector3(
                    startX + x * tileSpacing,
                    0f,
                    startZ - y * tileSpacing
                );

                gridTiles[x, y] = newTile;
            }
        }
    }

    public Tile[,] GetGridTiles()
    {
        return gridTiles;
    }

    private void ClearGrid()
    {
        if (gridParent == null)
            return;

        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            Destroy(gridParent.GetChild(i).gameObject);
        }
    }
}
