using UnityEngine;

/// <summary>
/// Scriptable Object for all attacks
/// contains all data an attack would need
/// </summary>
[CreateAssetMenu(fileName = "AttackSO", menuName = "svdrl21/AttackSO", order = 0)]
public class AttackSO : ScriptableObject
{
    public GameObject spawnPrefab;
    public GameObject launchEffectPrefab;
    public float launchVel = 0;
    public float minDist = -1;
    public float maxDist = -1;
    public float cooldown = 0;
    public float individualCoolDown = 0;
    public float moveBlockDur = 0;
    public bool keepAttached = false;
    public string animName = "";

    /// <summary>
    /// Triggers the actionable parts of the Attack
    /// spawns the prefab, launcheffect, and triggers animation
    /// </summary>
    /// <param name="parent">go to parent spawn to</param>
    /// <param name="spawnPoint">place to spawn</param>
    /// <param name="anim"></param>
    public void Trigger(Transform parent = null, Transform spawnPoint = null, Animator anim = null)
    {
        // only non writeable things can be in here
        if (spawnPrefab)
        {
            var spawnGo = Instantiate(spawnPrefab);
            spawnGo.transform.position = spawnPoint.position;
            spawnGo.transform.rotation = spawnPoint.rotation;
            if (keepAttached)
            {
                spawnGo.transform.SetParent(parent);
            }
            // todo launch dist
        }
        if (launchEffectPrefab)
        {
            var spawnGo = Instantiate(launchEffectPrefab);
            spawnGo.transform.position = spawnPoint.position;
            spawnGo.transform.rotation = spawnPoint.rotation;
        }
        if (animName.Length > 0)
        {
            anim.SetTrigger(animName);
        }
    }
}