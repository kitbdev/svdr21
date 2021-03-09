using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    // todo loading

    Transform spawnPoint;
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

    }
}
