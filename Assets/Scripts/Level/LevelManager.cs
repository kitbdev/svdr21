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
    public UnityEvent restartReadyEvent;
    public UnityEvent levelCompleteEvent;
    public UnityEvent levelReadyEvent;
    [ReadOnly] [SerializeField] LevelComponent startDoor;
    [ReadOnly] [SerializeField] LevelComponent lastEndDoor;
    [ReadOnly] [SerializeField] GameObject curStairsRoom;

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
    LevelComponent curDoor => curLevel == 0 ? startDoor : lastEndDoor;

    public int maxLevel => levels.Length - 1;

    [ReadOnly] public bool levelLoading = false;
    [ReadOnly] public bool playerIsSafe = false;
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
        playerIsSafe = true;
    }
    private void OnEnable()
    {
        LevelGen.Instance.GenCompleteEvent.AddListener(OnLevelLoaded);
    }
    private void OnDisable()
    {
        LevelGen.Instance?.GenCompleteEvent.RemoveListener(OnLevelLoaded);
    }
    [ContextMenu("Level complete")]
    public void LevelComplete()
    {
        // aka EnteredStairsRoom
        levelCompleteEvent.Invoke();
        // start loading next level
        LoadNextLevel();
        playerIsSafe = true;
        // todo everything in there needs to move seamlessly
        // Vector3 initialPos = curStairsRoom.transform.position;
        // curStairsRoom.transform.position -= initialPos;
        // GameManager.Instance.player.transform.position -= initialPos;
        // move to 0,0,0 - otherwise can go off to inf and crash
        // move stairs and move player relatively
        // todo 
        // ? make sure player is in a valid area?
    }
    // called by player on death to trigger a respawn
    public void LevelFail()
    {
        LoadMainRoom();
        // respawn player
        restartReadyEvent.Invoke();
        UnloadLevel();
        // todo intersecting?
        // make sure this will be fine for main room
    }
    public void LeftMainRoom()
    {
        // close door
        startDoor.connectedComponent?.GetComponent<Door>()?.EndInteract();
        UnloadMainRoom();
        playerIsSafe = false;
    }
    public void LeftStairsRoom()
    {
        // close door
        lastEndDoor.connectedComponent?.GetComponent<Door>()?.EndInteract();
        playerIsSafe = false;
        // the stairs room will get unloaded with the next level
    }
    void LoadMainRoom()
    {
        VRDebug.Log("Loading Main Room");
        mainRoom.SetActive(true);
        playerIsSafe = true;
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
        // todo unload current level except for endroom
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
        LevelGen.Instance.GenerateLevel(levels[curLevel], curDoor);
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
        VRDebug.Log("Opening door");
        if (curLevel == 0)
        {
            startDoor.myRoom.LevelStart();
        }
        curDoor.GetComponent<Door>().OpenDoor();
        levelReadyEvent.Invoke();
    }
    void UnloadLevel()
    {
        // todo enemies
        VRDebug.Log("Level " + curLevel + " unloading...");
        LevelGen.Instance.ClearLevel();
    }
}
