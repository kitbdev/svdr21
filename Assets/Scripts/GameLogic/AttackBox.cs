using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// hurts IHittables in trigger
/// </summary>
public class AttackBox : MonoBehaviour
{
    public string owner = "Player";
    public string checkTag = "Player";
    public bool isOn = true;
    // public Vector3 point;
    public Vector3 vel;
    public float hitDamage = 1;
    public float hitRepeatDelay = 1;
    float lastHitTime = 0;

    [ReadOnly] public HitArgs lastHitArgs;
    public UnityEvent hitEvent;

    private void OnTriggerEnter(Collider other)
    {
        CheckHit(other);
    }
    private void OnTriggerStay(Collider other)
    {
        CheckHit(other);
    }
    void CheckHit(Collider other)
    {
        if (!isOn)
        {
            return;
        }
        if (hitRepeatDelay >= 0 && lastHitTime > 0 && Time.time < lastHitTime + hitRepeatDelay)
        {
            return;
        }
        if (checkTag.Length == 0 || other.CompareTag(checkTag))
        {
            if (other.gameObject.TryGetComponent<IHittable>(out var hittable))
            {
                HitArgs args = new HitArgs();
                args.isDirect = true;
                args.damage = hitDamage;
                args.attacker = owner;
                args.point = transform.position;
                args.velocity = vel;
                hittable.Hit(args);

                lastHitArgs = args;
                lastHitTime = Time.time;
                hitEvent.Invoke();
            }
        }
    }
}