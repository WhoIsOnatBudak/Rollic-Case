using System;

[Serializable]
public class LevelData
{
    public int gridWidth;
    public int gridHeight;
    public int waitingAreaLength;
    public BusData[] buses;
    public GridCellData[] grid;
}

[Serializable]
public class BusData
{
    public string color;
}

[Serializable]
public class GridCellData
{
    public int x;
    public int y;

    public string contentType;
    public string color;
    public string direction;
    public string[] spawnColors;
}
