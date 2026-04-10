using System.Collections.Generic;
using UnityEngine;

public class WaitingAreaManager : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform waitingAreaParent;
    [SerializeField] private float tileSpacing = 1.2f;

    private List<Tile> waitingAreaTiles = new List<Tile>();

    public void CreateWaitingArea(int length)
    {
        ClearWaitingArea();
        waitingAreaTiles.Clear();

        float startX = -(length - 1) * tileSpacing / 2f;

        for (int i = 0; i < length; i++)
        {
            Tile waitingTile = Instantiate(tilePrefab, waitingAreaParent);

            waitingTile.transform.localPosition = new Vector3(
                startX + i * tileSpacing,
                0f,
                0f
            );

            waitingAreaTiles.Add(waitingTile);
        }
    }

    public List<Tile> GetWaitingAreaTiles()
    {
        return waitingAreaTiles;
    }

    public bool IsFull()
    {
        foreach (Tile tile in waitingAreaTiles)
        {
            if (tile.IsEmpty())
                return false;
        }
        return true;
    }

    public Tile GetFirstEmptyTile()
    {
        foreach (Tile tile in waitingAreaTiles)
        {
            if (tile.IsEmpty())
                return tile;
        }
        return null;
    }

    private void ClearWaitingArea()
    {
        if (waitingAreaParent == null)
            return;

        for (int i = waitingAreaParent.childCount - 1; i >= 0; i--)
        {
            Destroy(waitingAreaParent.GetChild(i).gameObject);
        }
    }
}
