using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [ReadOnly] [SerializeField] List<LevelOptionalComponent> allLevelComponents = new List<LevelOptionalComponent>();
    [ReadOnly] [SerializeField] List<LevelOptionalComponent> activeLevelComponents = new List<LevelOptionalComponent>();
    [ReadOnly] public List<LevelOptionalComponent> allConnectors = new List<LevelOptionalComponent>();
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
        var lcs = GetComponentsInChildren<LevelOptionalComponent>(true);
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
