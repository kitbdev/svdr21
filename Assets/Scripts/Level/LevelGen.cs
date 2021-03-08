using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Level generator
/// </summary>
public class LevelGen : Singleton<LevelGen>
{
    /*
    generation method
    starts with start room
    connect a new room to that ones possible opening
    */

    [Header("General gen settings")]
    public int maxTries = 10;
    public int maxRooms = 50;
    public int minSequentialRooms = 10;
    public LayerMask levelOnlyLayer;

    [Header("Prefabs")]
    public GameObject startRoomPrefab;
    public GameObject finishRoomPrefab;
    public GameObject[] roomPrefabs = new GameObject[0];

    [Header("Dynamic")]
    [ReadOnly] public List<Room> placedRooms = new List<Room>();
    // gates
    // keys
    /// <summary>connectors that havent been used</summary>
    [ReadOnly] [SerializeField] List<LevelOptionalComponent> frontierConnectors = new List<LevelOptionalComponent>();
    public bool genOnStart = true;

    private void Start()
    {
        if (genOnStart)
        {
            GenerateLevel();
        }
    }
    public void ClearLevel()
    {
        foreach (var room in placedRooms)
        {
            Destroy(room);
        }
        placedRooms.Clear();
        frontierConnectors.Clear();
        // remove all children
        int numChildren = transform.childCount;
        for (int i = numChildren - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    public void GenerateLevel()
    {
        StartCoroutine(GenLevelCo());
    }
    IEnumerator GenLevelCo()
    {
        yield return null;
        // clear
        ClearLevel();
        ValidatePrefabs();
        // start
        SpawnAndAddRoom(startRoomPrefab, Vector3.zero, Quaternion.identity);
        var firstConnector = placedRooms[0].allConnectors[0];
        SpawnRoomFor(firstConnector);
        // frontierConnectors.AddRange();
        // choose one to continue off of
        // int fcr = Random.Range(0, frontierConnectors.Count);
        // todo
    }
    void ValidatePrefabs()
    {
        foreach (var roomP in roomPrefabs)
        {
            Room prefabRoom = roomP.GetComponent<Room>();
            prefabRoom.FindAllLevelComponents();
        } //todo also start and end
    }
    void SpawnRoomFor(LevelOptionalComponent connector)
    {
        // try to spawn a room
        List<int> checkedPrefabIs = new List<int>();
        List<int> checkedConnectors = new List<int>();
        int tries = 0;
        while (tries < maxTries)
        {
            tries++;
            // select room randomly
            var rroomp = GetRandomIn<GameObject>(roomPrefabs, out int rrp, checkedPrefabIs);
            if (rrp == -1)
            {
                // all rooms tried!
                break;
            }
            checkedPrefabIs.Add(rrp);
            Room prefabRoom = rroomp.GetComponent<Room>();
            // try all connectors on the room
            checkedConnectors.Clear();
            int selCon = 0;
            while (selCon >= 0)
            {
                // select a connector
                var rCon = GetRandomIn<LevelOptionalComponent>(prefabRoom.allConnectors.ToArray(), out selCon, checkedConnectors);
                if (selCon == -1)
                {
                    // all components tried
                    break;
                }
                checkedConnectors.Add(selCon);
                // check room collision
                // todo
                Vector3 roomOffset = default;
                Quaternion roomRot = rCon.transform.rotation;
                if (IsValidRoomCol(prefabRoom.bounds, roomOffset, roomRot))
                {
                    // spawn the room
                    Room nroom = SpawnAndAddRoom(rroomp, roomOffset, roomRot);
                    // remove the used connector from frontier
                    frontierConnectors.Remove(connector);
                    // add new connectors, if any
                    frontierConnectors.AddRange(nroom.allConnectors);
                    frontierConnectors.Remove(rCon);
                    // var unusedCons = new List<LevelOptionalComponent>(nroom.allConnectors);
                    // if (unusedCons.Count > 0)
                    // {
                    //     frontierConnectors.AddRange(unusedCons);
                    // }
                    return;
                }
            }
        }
        // failed!
        Debug.LogError("SpawnRoomFor failed " + tries + " times!");
    }
    bool IsValidRoomCol(Bounds bounds, Vector3 roomOffset = default, Quaternion roomOrientation = default, List<Collider> ignoreCols = default)
    {
        // todo make sure room we are checking is not hit, or is ignored
        // overlap box
        var cols = Physics.OverlapBox(bounds.center + roomOffset, bounds.extents / 2, roomOrientation, levelOnlyLayer, QueryTriggerInteraction.Ignore);
        if (ignoreCols != null && ignoreCols.Count > 0)
        {
            // remove cols
            cols = ListSubtract<Collider>(cols, ignoreCols.ToArray());
        }
        if (cols.Length > 0)
        {
            return false;
        } else
        {
            return true;
        }
    }
    Room SpawnAndAddRoom(GameObject roomPrefab, Vector3 position, Quaternion rotation)
    {
        var t = SpawnRoomAt(roomPrefab, position, rotation);
        var r = t.GetComponent<Room>();
        placedRooms.Add(r);
        return r;
    }
    Transform SpawnRoomAt(GameObject roomPrefab, Vector3 position, Quaternion rotation)
    {
        var roomGo = Instantiate(roomPrefab, transform);
        roomGo.transform.position = position;
        roomGo.transform.rotation = rotation;
        return roomGo.transform;
    }


    // utility stuff
    /// <summary>
    /// Gets random item in the list or array
    /// </summary>
    /// <param name="array">list or array</param>
    /// <typeparam name="T"></typeparam>
    /// <param name="rIndex">the index out</param>
    /// <param name="ignoreIndices">indeces to ignore</param>
    /// <returns>random item in list</returns>
    static T GetRandomIn<T>(T[] array, out int rIndex, List<int> ignoreIndices = null)
    {
        if (ignoreIndices != null && ignoreIndices.Count > 0)
        {
            List<int> possibleIndices = new List<int>();
            for (int i = 0; i < array.Length; i++)
            {
                if (!ignoreIndices.Contains(i))
                {
                    possibleIndices.Add(i);
                }
            }
            if (possibleIndices.Count == 0)
            {
                // Debug.LogWarning("all possible indices ignored! " + typeof(T) + " " + array.Length);
                rIndex = -1;
                return array[0];
            } else
            {
                rIndex = possibleIndices[Random.Range(0, possibleIndices.Count)];
            }
        } else
        {
            rIndex = Random.Range(0, array.Length);
        }
        return array[rIndex];
    }
    static T GetRandomIn<T>(T[] list)
    {
        return GetRandomIn<T>(list, out var _);
    }
    static T[] ListSubtract<T>(T[] array1, T[] array2)
    {
        return ListSubtract<T>(array1, array2);
    }
    static List<T> ListSubtract<T>(List<T> list1, List<T> list2)
    {
        List<T> results = new List<T>(list1);
        results.Find((a) => !list2.Contains(a));
        return results;
    }
}
