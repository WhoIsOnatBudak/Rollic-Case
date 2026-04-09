using UnityEngine;

public abstract class TileContent : MonoBehaviour
{
    protected Tile ownerTile;

    public virtual void Initialize(Tile tile)
    {
        ownerTile = tile;
    }

    public Tile GetOwnerTile()
    {
        return ownerTile;
    }

    public void SetOwnerTile(Tile tile)
    {
        ownerTile = tile;
    }


}
