using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [ReadOnly] [SerializeField] List<LevelComponent> allLevelComponents = new List<LevelComponent>();
    [ReadOnly] [SerializeField] List<LevelComponent> activeLevelComponents = new List<LevelComponent>();
    [ReadOnly] public List<LevelComponent> allConnectors = new List<LevelComponent>();
    public int uniqueToLevel = -1;

    public Bounds bounds
    {
        get {
            if (cacheCol == null)
            {
                cacheCol = GetComponent<Collider>();
            }
            return cacheCol.bounds;
        }
    }
    Collider cacheCol;
    // todo disable collider when starting

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
        }
        allLevelComponents.AddRange(lcs);
    }
}
