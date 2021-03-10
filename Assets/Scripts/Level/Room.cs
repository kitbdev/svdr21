using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Room used by level gen
/// </summary>
// [SelectionBase]
public class Room : MonoBehaviour
{
    public int uniqueToLevel = -1;
    public bool isPathRoom = false;
    // updated before level gen, in prefab
    [ReadOnly] public List<LevelComponent> allLevelComponents = new List<LevelComponent>();
    [ReadOnly] [SerializeField] List<LevelComponent> usedLevelComponents = new List<LevelComponent>();
    [ReadOnly] public List<LevelComponent> allConnectors = new List<LevelComponent>();
    // updated during level gen
    [ReadOnly] public List<Room> connectedRooms = new List<Room>();
    [ReadOnly] public List<LevelComponent> blockedConnectors = new List<LevelComponent>();
    [ReadOnly] public bool hasKey = false;
    // dynamic
    public List<LevelComponent> reqLevelComponents => allLevelComponents.FindAll(lc => {
        return !lc.isRoomConnector && !lc.isInUse && lc.isRequired;
    });
    // any non door, and non chest! 
    public List<LevelComponent> normalLevelComponents => allLevelComponents.FindAll(lc => {
        return !lc.isRoomConnector && !lc.isInUse && !lc.isRequired && !blockedConnectors.Contains(lc);
    });
    // todo special ones too

    public UnityEvent roomStartEvent;

    /// <summary>
    /// bounds should cover the majority of the level
    /// only connectors and parts that can intersect with other roomsshould be outside
    /// </summary>
    public Bounds GetBounds()
    {
        CheckCol();
        var b = new Bounds(cacheCol.center, cacheCol.size);
        // Debug.Log("col: " + cacheCol.center.ToString());
        return b;
    }
    BoxCollider cacheCol;
    void CheckCol()
    {
        if (cacheCol == null)
        {
            cacheCol = GetComponent<BoxCollider>();
            if (cacheCol == null)
                cacheCol = GetComponentInChildren<BoxCollider>();
        }
    }

    protected void Awake()
    {
        FindAllLevelComponents();
    }
    [ContextMenu("clear components")]
    public void ClearLevelComponents()
    {
        allLevelComponents.Clear();
        allConnectors.Clear();
        // connectedRooms.Clear();
        // blockedConnectors.Clear();
        // hasKey = false;
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
        CheckCol();
        cacheCol.enabled = false;
        roomStartEvent.Invoke();
        foreach (var lc in allLevelComponents)
        {
            lc.SetUsing(usedLevelComponents.Contains(lc));
        }
        // todo?
    }
    public bool TryUseLComponent(LevelComponent component)
    {
        if (component.isInUse)
        {
            return true;
        }
        if (!CanUseLComponent(component))
        {
            // we know we cant use this, so block it anyway
            blockedConnectors.Add(component);
            return false;
        }
        ForceUseLComponent(component);
        // ! doors can not block doors, only other stuff
        return true;
    }
    public void ForceUseLComponent(LevelComponent component)
    {
        if (component.isInUse)
        {
            return;
        }
        blockedConnectors.AddRange(blockedConnectors);
        component.isInUse = true;
        usedLevelComponents.Add(component);
        foreach (var forceC in component.forceUse)
        {
            TryUseLComponent(forceC);
        }
        if (component.requireOneOf.Count > 0)
        {
            var choosenOne = LevelGen.GetRandomIn<LevelComponent>(component.requireOneOf.ToArray(), out int rofi);
            TryUseLComponent(choosenOne);
        }
    }
    List<LevelComponent> checkedCs = new List<LevelComponent>();
    public bool CanUseLComponent(LevelComponent component)
    {
        // recursion
        checkedCs.Clear();
        return CanUseLComponentRec(component);
    }
    bool CanUseLComponentRec(LevelComponent component)
    {
        if (checkedCs.Contains(component))
        {
            // we already checked or are checking this, so say this is valid
            return true;
        }
        checkedCs.Add(component);
        // ? pass toblock list and forceuse list
        if (blockedConnectors.Contains(component))
        {
            // we are blocked by someone else
            return false;
        }
        // blocks a components
        foreach (var blockC in component.blocks)
        {
            // cant block a component already in use
            if (blockC.isInUse)
            {
                return false;
            }
        }
        // are any use one of valid
        bool anyAvailable = component.requireOneOf.Count > 0;
        foreach (var reqOne in component.requireOneOf)
        {
            if (CanUseLComponentRec(reqOne))
            {
                anyAvailable = true;
                break;
            }
        }
        if (anyAvailable)
        {
            return false;
        }
        // force another component
        foreach (var forceC in component.forceUse)
        {
            if (!CanUseLComponentRec(forceC))
            {
                // ! chains of blocks and requires not supported
                return false;
            }
        }
        return true;
    }
}
