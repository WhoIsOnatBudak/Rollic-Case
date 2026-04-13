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

    public bool AddExtraTile()
    {
        // n tile varken yeni merkez -n*spacing/2 olacak, her tile spacing/2 sola kayar
        float shift = -tileSpacing / 2f;

        foreach (Tile tile in waitingAreaTiles)
        {
            tile.transform.localPosition += new Vector3(shift, 0f, 0f);

            // Tile üzerindeki yolcu varsa onu da kaydır
            if (!tile.IsEmpty() && tile.GetContent() is PassengerContent passenger)
                passenger.MoveTo(tile.transform.position, 8f);
        }

        // Yeni tile sağ uca yerleşir: n*spacing/2
        float newX = waitingAreaTiles.Count * tileSpacing / 2f;

        Tile waitingTile = Instantiate(tilePrefab, waitingAreaParent);
        waitingTile.transform.localPosition = new Vector3(newX, 0f, 0f);
        waitingAreaTiles.Add(waitingTile);
        return true;
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
