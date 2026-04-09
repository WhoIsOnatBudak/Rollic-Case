using UnityEngine;

public class Tile : MonoBehaviour
{
    private int x;
    private int y;

    private TileContent content;

    public void Initialize(int xValue, int yValue)
    {
        x = xValue;
        y = yValue;
        content = null;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public TileContent GetContent()
    {
        return content;
    }

    public void SetContent(TileContent newContent)
    {
        content = newContent;

        if (content != null)
            content.SetOwnerTile(this);
    }

    public void ClearContent()
    {
        content = null;
    }

    public bool IsWalkable()
    {
        if (content == null)
            return true;

        return false;
    }

    public bool IsEmpty()
    {
        return content == null;
    }
}