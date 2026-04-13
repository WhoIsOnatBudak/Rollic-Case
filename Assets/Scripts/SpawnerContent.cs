using UnityEngine;
using System.Collections.Generic;

public enum TileDirection { Up, Down, Left, Right }

public class SpawnerContent : TileContent
{
    [SerializeField] private TMPro.TextMeshPro countText;
    
    private TileDirection facingDirection;
    private Queue<ColorType> passengerQueue = new Queue<ColorType>();
    private PassengerContent passengerPrefab;
    private Transform contentParent;

    public void InitializeSpawner(string directionStr, string[] colors, PassengerContent prefab, Transform parent)
    {
        switch (directionStr)
        {
            case "Up": facingDirection = TileDirection.Up; break;
            case "Down": facingDirection = TileDirection.Down; break;
            case "Left": facingDirection = TileDirection.Left; break;
            case "Right": facingDirection = TileDirection.Right; break;
            default: facingDirection = TileDirection.Down; break;
        }

        passengerPrefab = prefab;
        contentParent = parent;

        if (colors != null)
        {
            foreach (string colorStr in colors)
            {
                passengerQueue.Enqueue(ParseColorType(colorStr));
            }
        }

        ApplyDirectionRotation();

        UpdateUI();
    }

    private void ApplyDirectionRotation()
    {
        Vector3 lookDir = Vector3.forward;
        switch (facingDirection)
        {
            case TileDirection.Up: lookDir = Vector3.right; break;
            case TileDirection.Down: lookDir = Vector3.left; break;
            case TileDirection.Left: lookDir = Vector3.forward; break;
            case TileDirection.Right: lookDir = Vector3.back; break;
        }

        Vector3 textOriginalPosition = Vector3.zero;
        Quaternion textOriginalRotation = Quaternion.identity;
        bool hasText = (countText != null && countText.transform != this.transform);

        // Save original position and rotation of the text element
        if (hasText)
        {
            //textOriginalPosition = countText.transform.position;
            textOriginalRotation = countText.transform.rotation;
        }

        // Rotate the spawner root to face the desired direction
        
        transform.rotation = Quaternion.LookRotation(lookDir);
        

        // Restore the original world position and rotation to the text so it remains static
        if (hasText)
        {
            //countText.transform.position = textOriginalPosition;
            countText.transform.rotation = textOriginalRotation;
        }
    }

    public TileDirection GetFacingDirection()
    {
        return facingDirection;
    }

    public bool HasPassengers()
    {
        return passengerQueue.Count > 0;
    }

    public void ReturnPassenger(ColorType color)
    {
        // Rengi queue'nun başına ekle (undo için)
        Queue<ColorType> newQueue = new Queue<ColorType>();
        newQueue.Enqueue(color);
        while (passengerQueue.Count > 0)
            newQueue.Enqueue(passengerQueue.Dequeue());
        passengerQueue = newQueue;
        UpdateUI();
    }

    public void SpawnToTile(Tile targetTile)
    {
        if (passengerQueue.Count == 0 || targetTile == null || !targetTile.IsEmpty())
            return;

        ColorType nextColor = passengerQueue.Dequeue();
        UpdateUI();

        PassengerContent passenger = Instantiate(passengerPrefab, targetTile.transform.position, Quaternion.identity, contentParent);
        passenger.SetColor(nextColor);
        passenger.Initialize(targetTile);
        targetTile.SetContent(passenger);
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = passengerQueue.Count.ToString();
            if (passengerQueue.Count == 0)
                countText.text = "";
        }
    }

    private ColorType ParseColorType(string colorString)
    {
        switch (colorString)
        {
            case "Red": return ColorType.Red;
            case "Blue": return ColorType.Blue;
            case "Green": return ColorType.Green;
            case "Yellow": return ColorType.Yellow;
            default: return ColorType.Red;
        }
    }

    public static void CheckAndTriggerAdjacentSpawners(Tile vacatedTile, GridManager gridManager)
    {
        if (vacatedTile == null || gridManager == null) return;
        
        int x = vacatedTile.GetX();
        int y = vacatedTile.GetY();

        // Spawner below facing Up
        CheckSpawnerAt(x, y + 1, TileDirection.Up, vacatedTile, gridManager);
        // Spawner above facing Down
        CheckSpawnerAt(x, y - 1, TileDirection.Down, vacatedTile, gridManager);
        // Spawner to the right facing Left
        CheckSpawnerAt(x + 1, y, TileDirection.Left, vacatedTile, gridManager);
        // Spawner to the left facing Right
        CheckSpawnerAt(x - 1, y, TileDirection.Right, vacatedTile, gridManager);
    }

    private static void CheckSpawnerAt(int spawnerX, int spawnerY, TileDirection expectedDirection, Tile targetTile, GridManager gridManager)
    {
        Tile adjTile = gridManager.GetTile(spawnerX, spawnerY);
        if (adjTile != null && adjTile.GetContent() is SpawnerContent spawner)
        {
            if (spawner.GetFacingDirection() == expectedDirection)
            {
                spawner.SpawnToTile(targetTile);
            }
        }
    }
}
