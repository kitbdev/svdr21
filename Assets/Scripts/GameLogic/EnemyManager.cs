using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Keep track of all enemies
/// spawn them when needed?
/// </summary>
public class EnemyManager : Singleton<EnemyManager>
{
    public GameObject[] allEnemyPrefabs = new GameObject[0];

    [Space]
    [ReadOnly] public List<EnemyAI> activeEnemies = new List<EnemyAI>();
    // [ReadOnly] public List<Trap> allTraps = new List<Trap>();
    private void OnEnable()
    {
        LevelManager.Instance.levelCompleteEvent.AddListener(DespawnAllEnemies);
        LevelManager.Instance.levelReadyEvent.AddListener(SpawnAllEnemies);
    }
    private void OnDisable()
    {
        LevelManager.Instance?.levelCompleteEvent.RemoveListener(DespawnAllEnemies);
        LevelManager.Instance?.levelReadyEvent.RemoveListener(SpawnAllEnemies);
    }
    public void DespawnAllEnemies()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            Destroy(activeEnemies[i].gameObject);
        }
        activeEnemies.Clear();
    }
    public void SpawnAllEnemies()
    {
        // find all spawnlocations
        VRDebug.Log("Spawning Enemies");
        string spawnLocString = "EnemySpawnPoint";
        var splocs = GameObject.FindGameObjectsWithTag(spawnLocString);
        // just spawn one at each
        foreach (var sploc in splocs)
        {
            SpawnEnemy(allEnemyPrefabs[0], sploc.transform);
        }
    }
    public void SpawnEnemy(GameObject enemyPrefab, Transform location)
    {
        // todo pooling?
        GameObject eGo = Instantiate(enemyPrefab, transform);
        eGo.transform.position = location.position;
        eGo.transform.rotation = location.rotation;
        EnemyAI eAi = eGo.GetComponent<EnemyAI>();
        activeEnemies.Add(eAi);
    }
    public void PlayerDefeated()
    {
        // tell all enemies
        foreach (var eai in activeEnemies)
        {
            eai.PlayerDefeated();
        }
    }
    public void EnemyDied(EnemyAI enemy)
    {
        activeEnemies.Remove(enemy);
    }
}