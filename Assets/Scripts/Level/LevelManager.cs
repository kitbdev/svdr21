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
    public UnityEvent levelReadyEvent;

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
    }
    private void Start()
    {
        if (loadOnStart)
        {
            LoadLevel();
        }
    }
    private void OnEnable()
    {
        LevelGen.Instance.GenCompleteEvent.AddListener(OnLevelLoaded);
    }
    private void OnDisable()
    {
        LevelGen.Instance.GenCompleteEvent.RemoveListener(OnLevelLoaded);
    }
    public void LevelComplete()
    {
        // start next level
        LoadNextLevel();
        // prepare player?
    }
    void LoadNextLevel()
    {
        curLevel++;
        if (curLevel >= levels.Length)
        {
            VRDebug.Log("Reached last level!");
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
        LevelGen.Instance.GenerateLevel(levels[curLevel]);
    }
    void OnLevelLoaded()
    {
        VRDebug.Log("Level " + curLevel + " finished loading");
        levelLoading = false;
        levelReadyEvent.Invoke();
        // todo something
    }
}
