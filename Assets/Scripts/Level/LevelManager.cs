using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public LevelGenSettings[] levels = new LevelGenSettings[0];
    [SerializeField]
    private int m_curLevel = 0;

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
    }
    private void Start()
    {
        curLevel = 0;
        LoadLevel();
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
        // start next level
        LoadNextLevel();
        // prepare player?
    }
    void LoadNextLevel()
    {
        curLevel++;
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
        Debug.Log("Level " + curLevel + " loading...");
        LevelGen.Instance.GenerateLevel(levels[curLevel]);
    }
    void OnLevelLoaded()
    {
        Debug.Log("Level " + curLevel + " finished loading");
        levelLoading = false;
        // todo something
    }
}
