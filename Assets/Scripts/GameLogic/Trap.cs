using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// like an enemy, attacks the player using an AttackSO
/// only one attack
/// cannot move
/// </summary>
public class Trap : MonoBehaviour
{
    public bool isOn = true;
    public AttackSO attack;
    public float minPlayerDist = -1;
    float lastAttackTime = 0;
    Transform player;

    private void Awake()
    {
        player = GameManager.Instance.player;
    }
    public void SetOn(bool isNowOn)
    {
        isOn = isNowOn;
    }
    private void Update()
    {
        if (isOn)
        {
            TryToAttack();
        }
    }
    void TryToAttack()
    {
        if (Time.time < lastAttackTime + attack.cooldown)
        {
            return;
        }
        if (minPlayerDist > 0)
        {
            if (Vector3.Distance(transform.position, player.position) > minPlayerDist)
            {
                return;
            }
        }
        TriggerAttack();
    }
    void TriggerAttack()
    {
        attack.Trigger(transform, transform);
        lastAttackTime = Time.time;
    }
}