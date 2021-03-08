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
    public GameObject endRoomPrefab;
    public GameObject[] roomPrefabs = new GameObject[0];

    // debug stuff
    public bool genOnStart = true;
    public bool advancedDebug = false;

    [Header("Dynamic")]
    [ReadOnly] public int numRooms = 0;
    [ReadOnly] [SerializeField] List<Room> placedRooms = new List<Room>();
    [ReadOnly] [SerializeField] List<Room> mainPath = new List<Room>();
    // gates
    // keys
    /// <summary>connectors that havent been used</summary>
    [ReadOnly] [SerializeField] List<LevelOptionalComponent> frontierConnectors = new List<LevelOptionalComponent>();
    [ReadOnly] [SerializeField] List<LevelOptionalComponent> lastRoomUnusedCons = new List<LevelOptionalComponent>();

    private void Start()
    {
        if (genOnStart)
        {
            GenerateLevel();
        }
    }
    [ContextMenu("Clear Level")]
    public void ClearLevel()
    {
        Debug.ClearDeveloperConsole();
        Debug.Log("clearing level");
        numRooms = 0;
        foreach (var room in placedRooms)
        {
            SafeDestroy(room.gameObject);
        }
        placedRooms.Clear();
        mainPath.Clear();
        frontierConnectors.Clear();
        lastRoomUnusedCons.Clear();
        // remove all children?
        // int numChildren = transform.childCount;
        // for (int i = numChildren - 1; i >= 0; i--)
        // {
        //     Destroy(transform.GetChild(i).gameObject);
        // }
    }
    void SafeDestroy(Object obj)
    {
#if UNITY_EDITOR
        DestroyImmediate(obj);
#else
        Destroy(obj);
#endif
    }
    [ContextMenu("Gen Level")]
    public void GenerateLevel()
    {
        StartCoroutine(GenLevelCo());
    }
    IEnumerator GenLevelCo()
    {
        // clear
        ClearLevel();
        ValidatePrefabs();
        yield return null;
        // start
        Debug.Log("level gen start");
        SpawnAndAddRoom(startRoomPrefab, Vector3.zero, Quaternion.identity);
        var firstConnector = placedRooms[0].allConnectors[0];
        SpawnRoomFor(firstConnector);

        List<int> checkedFConIs = new List<int>();
        while (numRooms <= maxRooms)
        {
            if (frontierConnectors.Count == 0)
            {
                Debug.LogWarning("Out of all frontierConnectors!");
                break;
            }
            if (lastRoomUnusedCons.Count == 0)
            {
                Debug.Log("Dead end! out of lastRoomUnusedCons");
                if (!FillUnusedCons())
                {
                    Debug.LogError("No prior rooms have connectors!");
                }
                break;
            }
            bool useEndRoom = numRooms == maxRooms;
            // try all connections
            int nextConIndex = 0;
            checkedFConIs.Clear();
            while (nextConIndex >= 0)
            {
                // continue linearly, so use an unused connector on the last room
                var nextCon = GetRandomIn<LevelOptionalComponent>(lastRoomUnusedCons.ToArray(), out nextConIndex, checkedFConIs);
                if (nextConIndex == -1)
                {
                    // tried all of these connectors
                    Debug.Log("Tried all connectors in lastRoomUnusedCons: " + lastRoomUnusedCons.Count);
                    if (!FillUnusedCons())
                    {
                        Debug.LogError("No prior rooms have connectors!");
                    }
                    continue;
                }
                if (useEndRoom ? SpawnRoomFor(nextCon, endRoomPrefab) : SpawnRoomFor(nextCon))
                {
                    // success
                    break;
                } else
                {
                    // continue to try another connector
                    continue;
                }
            }
            yield return null;
            // todo add additional rooms with lock and key structure
        }
        // done with spawning rooms
        yield return null;
        // randomize individual rooms
        // todo
        // add loot
        // todo
        // spawn enemies
        // todo
    }
    /// <summary>
    /// get unused connectors from the previous room
    /// moves main path back
    /// </summary>
    /// <returns>true on success</returns>
    bool FillUnusedCons()
    {
        int prevPathRoom = mainPath.Count - 2;
        if (prevPathRoom > 0)
        {
            lastRoomUnusedCons.Clear();
            // add that rooms connectors
            lastRoomUnusedCons.AddRange(mainPath[prevPathRoom].allConnectors);
            // remove the used ones,the ones not in frontier
            lastRoomUnusedCons.RemoveAll((a) => !frontierConnectors.Contains(a));
            // shorten main path
            mainPath.RemoveAt(prevPathRoom + 1);
            if (lastRoomUnusedCons.Count > 0)
            {
                return true;
            } else
            {
                // recursion
                return FillUnusedCons();
            }
        }
        return false;
    }
    /// <summary>
    /// try to spawn a room for a connector
    /// </summary>
    /// <param name="connector"></param>
    /// <param name="forceRoom"></param>
    /// <returns>true on success</returns>
    bool SpawnRoomFor(LevelOptionalComponent connector, GameObject forceRoom = null)
    {
        // try to spawn a room
        List<int> checkedRoomPrefabIs = new List<int>();
        List<int> checkedConnectorIs = new List<int>();
        checkedRoomPrefabIs.Clear();
        int tries = 0;
        // ? instead try all rooms
        while (tries < maxTries)
        {
            tries++;
            // select room randomly
            var rroomp = GetRandomIn<GameObject>(roomPrefabs, out int rrp, checkedRoomPrefabIs);
            if (rrp == -1)
            {
                // all rooms tried!
                Debug.LogWarning("Tried all rooms!");
                break;
            }
            checkedRoomPrefabIs.Add(rrp);
            if (forceRoom)
            {
                rroomp = forceRoom;
            }
            Debug.Log("trying to spawn room " + rroomp.name);
            Room prefabRoom = rroomp.GetComponent<Room>();
            // try all connectors on the room
            checkedConnectorIs.Clear();
            int selCon = 0;
            while (selCon >= 0)
            {
                // select a connector
                var rCon = GetRandomIn<LevelOptionalComponent>(prefabRoom.allConnectors.ToArray(), out selCon, checkedConnectorIs);
                if (selCon == -1)
                {
                    // all components tried
                    Debug.Log("Tried all connectors on " + rroomp.name);
                    break;
                }
                checkedConnectorIs.Add(selCon);
                // check room collision
                // todo check
                // wanted room postion 
                Vector3 roomOffset = connector.transform.position + -rCon.transform.localPosition;
                Quaternion roomRot = connector.transform.rotation * Quaternion.Inverse(rCon.transform.rotation);
                if (IsValidRoomCol(prefabRoom.bounds, roomOffset, roomRot))
                {
                    Debug.Log("room is valid");
                    // spawn the room
                    Room nroom = SpawnAndAddRoom(rroomp, roomOffset, roomRot);
                    mainPath.Add(nroom); // todo may not be a mainpath room
                    // remove the used connector from frontier
                    frontierConnectors.Remove(connector);
                    // add new connectors, if any
                    lastRoomUnusedCons.Clear();
                    lastRoomUnusedCons.AddRange(nroom.allConnectors);
                    lastRoomUnusedCons.RemoveAt(selCon);
                    if (lastRoomUnusedCons.Count > 0)
                    {
                        frontierConnectors.AddRange(lastRoomUnusedCons);
                    }
                    return true;
                }
            }
            if (forceRoom)
            {
                break;
            }
        }
        // failed to place a room on this connector!
        return false;
        // Debug.LogError("SpawnRoomFor failed " + tries + " times!");
    }
    bool IsValidRoomCol(Bounds bounds, Vector3 roomOffset = default, Quaternion roomOrientation = default, List<Collider> ignoreCols = default)
    {
        // overlap box
        var cols = Physics.OverlapBox(bounds.center + roomOffset, bounds.extents / 2, roomOrientation, levelOnlyLayer, QueryTriggerInteraction.Ignore);
        // make sure room we are checking can be ignored
        if (ignoreCols != null && ignoreCols.Count > 0)
        {
            // remove cols
            cols = ListSubtract<Collider>(cols, ignoreCols.ToArray());
        }
        if (cols.Length > 0)
        {
            if (advancedDebug)
            {
                Debug.Log($"invalid room b:{bounds.ToString()} p:{roomOffset} q:{roomOrientation.eulerAngles}");
                string logText = "intersecting ";
                for (int i = 0; i < cols.Length; i++)
                {
                    logText += cols[i].name;
                }
                Debug.Log(logText);
            }
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
    void ValidatePrefabs()
    {
        // ! be careful, this is editing the actual prefab
        Room startprefabRoom = startRoomPrefab.GetComponent<Room>();
        startprefabRoom.FindAllLevelComponents();
        if (endRoomPrefab)
        {
            Room endprefabRoom = endRoomPrefab.GetComponent<Room>();
            endprefabRoom.FindAllLevelComponents();
        }
        foreach (var roomP in roomPrefabs)
        {
            Room prefabRoom = roomP.GetComponent<Room>();
            prefabRoom.FindAllLevelComponents();
        } //todo also start and end
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
