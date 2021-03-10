using UnityEngine;

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
}