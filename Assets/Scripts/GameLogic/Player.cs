using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Player management code
/// respawning
/// </summary>
public class Player : MonoBehaviour
{
    // todo loading
    public float deathWaitMinDur = 5;
    float lastDeathTime = 0;
    Transform spawnPoint;
    public Bow bow;
    XRRig rig;
    Health health;

    private void Awake()
    {
        rig = GetComponent<XRRig>();
        health = GetComponent<Health>();
        health.dieEvent.AddListener(Die);
        health.damageEvent.AddListener(Damaged);
    }
    private void OnEnable()
    {
        LevelManager.Instance.levelReadyEvent.AddListener(Respawn);
    }
    private void OnDisable()
    {
        LevelManager.Instance.levelReadyEvent.RemoveListener(Respawn);
    }
    void Damaged()
    {
        VRDebug.Log("Player was hit");
    }
    void FindSpawnPoint()
    {
        // ? set from level manager instead
        spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn")?.transform;
    }
    public void Respawn()
    {
        VRDebug.Log("Player respawning");
        bow.canUse = true;
        health.RestoreHealth();
        if (!spawnPoint)
        {
            FindSpawnPoint();
        }
        if (spawnPoint)
        {
            rig.rig.transform.position = spawnPoint.position;
            rig.rig.transform.rotation = spawnPoint.rotation;
        } else
        {
            VRDebug.Log("No Spawn point found");
            rig.rig.transform.position = Vector3.zero;
            rig.rig.transform.rotation = Quaternion.identity;
        }
        // velocity?
        // remove thrown arrows?
        // todo reload scene instead?
        // arrows, enemies, lots of stuff
    }
    void Die()
    {
        VRDebug.Log("Player died");
        lastDeathTime = Time.time;
        Invoke("StartReloading", deathWaitMinDur);
        bow.canUse = false;
        EnemyManager.Instance.PlayerDefeated();
        // todo death state
        // cannot fire arrows, time frozen? enemies frozen or something
    }
    void StartReloading()
    {
        LevelManager.Instance.LoadMainRoom();
        // respawn will be triggered
    }
}
