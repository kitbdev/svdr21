using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [ReadOnly] public List<LevelComponent> allLevelComponents = new List<LevelComponent>();
    [ReadOnly] public List<LevelComponent> usedLevelComponents = new List<LevelComponent>();
    [ReadOnly] public List<LevelComponent> allConnectors = new List<LevelComponent>();
    public int uniqueToLevel = -1;
    public bool isPathRoom = false;
    [ReadOnly] public List<Room> connectedRooms = new List<Room>();

    /// <summary>
    /// bounds should cover the majority of the level
    /// only connectors and parts that can intersect with other roomsshould be outside
    /// </summary>
    /// <value></value>
    public Bounds bounds => cacheCol.bounds;
    Collider _cacheCol;
    public Collider cacheCol
    {
        get {
            if (_cacheCol == null)
                _cacheCol = GetComponent<Collider>();
            return _cacheCol;
        }
        set => _cacheCol = value;
    }

    private void Awake()
    {
        FindAllLevelComponents();
    }
    [ContextMenu("clear components")]
    public void ClearLevelComponents()
    {
        allLevelComponents.Clear();
        allConnectors.Clear();
    }
    [ContextMenu("find components")]
    public void FindAllLevelComponents()
    {
        ClearLevelComponents();
        var lcs = GetComponentsInChildren<LevelComponent>(true);
        foreach (var lc in lcs)
        {
            if (lc.isRoomConnector)
            {
                allConnectors.Add(lc);
            }
            lc.myRoom = this;
        }
        allLevelComponents.AddRange(lcs);
    }
    /// <summary>
    /// Prepare for the level to start
    /// </summary>
    public void LevelStart()
    {
        // disable bounds collider
        cacheCol.enabled = false;
        foreach (var lc in allLevelComponents)
        {
            lc.SetUsing(usedLevelComponents.Contains(lc));
        }
        // todo?
    }
}
