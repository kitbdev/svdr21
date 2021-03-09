using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitBox : MonoBehaviour, IHittable
{
    public float hitScale = 1;
    public UnityEvent<HitArgs> hitEvent = new UnityEvent<HitArgs>();

    public void Hit(HitArgs args)
    {
        // VRDebug.Log(name +" hit by " + args.attacker);
        args.hit = gameObject;
        args.damage *= hitScale;
        hitEvent.Invoke(args);
    }
}