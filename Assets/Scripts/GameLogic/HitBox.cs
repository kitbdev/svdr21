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
        args.damage *= hitScale;
        hitEvent.Invoke(args);
    }
}