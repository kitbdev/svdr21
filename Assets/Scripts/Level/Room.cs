using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [ReadOnly] [SerializeField] List<LevelOptionalComponent> allLevelComponents = new List<LevelOptionalComponent>();
    [ReadOnly] [SerializeField] List<LevelOptionalComponent> activeLevelComponents = new List<LevelOptionalComponent>();
    [ReadOnly] public List<LevelOptionalComponent> allConnectors = new List<LevelOptionalComponent>();
    public Bounds bounds;

    private void Awake()
    {
        FindAllLevelComponents();
    }
    [AddComponentMenu("find components")]
    public void FindAllLevelComponents()
    {
        allLevelComponents.Clear();
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
