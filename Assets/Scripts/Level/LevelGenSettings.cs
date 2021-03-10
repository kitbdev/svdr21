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
    public int maxGates = 3;
    public float gateChance = 30;

    public LevelGenSettings()
    {
        difficulty = 1;
        preferredRooms = 10;
        minTotalRooms = 3;
        minSequentialRooms = 3;
        maxGates = 3;
        gateChance = 30;
    }
}