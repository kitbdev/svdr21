using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// handles level switching, loading, and management
/// notifies when level is ready
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    public LevelGenSettings[] levels = new LevelGenSettings[0];
    public bool loadOnStart = true;
    [SerializeField]
    private int m_curLevel = 0;
    public GameObject mainRoom;
    public UnityEvent mainRoomReadyEvent;
    LevelComponent startDoor;
    LevelComponent lastEndDoor;

    public int curLevel
    {
        get => m_curLevel; set {
            if (levelLoading)
            {
                Debug.LogWarning("Cannot set curlevel, we are loading");
            } else
            {
                m_curLevel = value;
            }
        }
    }

    public int maxLevel => levels.Length - 1;

    [ReadOnly] public bool levelLoading = false;
    // float levelLoadStartTime = 0;

    protected override void Awake()
    {
        base.Awake();
        curLevel = 0;
        startDoor = mainRoom.GetComponent<Room>().allConnectors[0];
    }
    private void Start()
    {
        if (loadOnStart)
        {
            LoadMainRoom();
            // mainRoom.SetActive(false);
            // LoadLevel();
        }
    }
    private void OnEnable()
    {
        LevelGen.Instance.GenCompleteEvent.AddListener(OnLevelLoaded);
    }
    private void OnDisable()
    {
        LevelGen.Instance?.GenCompleteEvent.RemoveListener(OnLevelLoaded);
    }
    public void LevelComplete()
    {
        // start loadin next level
        LoadNextLevel();
    }
    // called by player on death to trigger a respawn
    public void LevelFail()
    {
        LoadMainRoom();
        // respawn player
        mainRoomReadyEvent.Invoke();
        UnloadLevel();
        // todo intersecting?
        // make sure this will be fine for main room
    }
    void LoadMainRoom()
    {
        VRDebug.Log("Loading Main Room");
        mainRoom.SetActive(true);
        // once in main room, start loading first level
        curLevel = 0;
        LoadLevel();
    }
    void UnloadMainRoom()
    {
        mainRoom.SetActive(false);
    }
    void LoadNextLevel()
    {
        curLevel++;
        if (curLevel >= levels.Length)
        {
            VRDebug.Log("Reached last level!");
            curLevel = levels.Length - 1;
            // todo infinite levels?
        }
        LoadLevel();
    }
    void LoadLevel()
    {
        if (levelLoading)
        {
            Debug.LogWarning("Already loading Level " + curLevel);
            return;
        }
        levelLoading = true;
        VRDebug.Log("Level " + curLevel + " loading...");
        LevelGen.Instance.GenerateLevel(levels[curLevel], curLevel == 0 ? startDoor : lastEndDoor);
    }
    void OnLevelLoaded()
    {
        VRDebug.Log("Level " + curLevel + " finished loading");
        levelLoading = false;

        // get the end door for the next level
        // there is no regenerating of cur level, so its fine to set here
        lastEndDoor = LevelGen.Instance.nextLevelDoor;
        // todo something
        // open main/end room door
        if (curLevel == 0)
        {
            startDoor.GetComponent<Door>().Interact();
        }
    }
    void UnloadLevel()
    {
        // todo enemies
        VRDebug.Log("Level " + curLevel + " unloading...");
        LevelGen.Instance.ClearLevel();
    }
}
