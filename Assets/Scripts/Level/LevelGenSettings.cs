using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Settings LevelGen will use to generate a level
/// allows for different types of levels to be generated
/// </summary>
[System.Serializable]
public class LevelGenSettings
{
    public int difficulty = 1;
    public int preferredRooms = 10;
    public int minTotalRooms = 3;
    // ignoring gates, the shortest path
    public int numMainRooms = 3;
    public int maxGates = 3;
    public float gateChance = 30;

    public LevelGenSettings()
    {
        difficulty = 1;
        preferredRooms = 10;
        minTotalRooms = 3;
        numMainRooms = 3;
        maxGates = 3;
        gateChance = 30;
    }
}