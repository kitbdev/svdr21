using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelGenSettings
{
    public int difficulty = 1;
    public int preferredRooms = 10;
    public int minTotalRooms = 3;
    public int minSequentialRooms = 3;

    public LevelGenSettings()
    {
        difficulty = 1;
        preferredRooms = 10;
        minTotalRooms = 3;
        minSequentialRooms = 3;
    }
}