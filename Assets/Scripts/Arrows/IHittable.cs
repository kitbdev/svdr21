using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitArgs
{
    public float damage;
    public bool isDirect;
    public string attacker;
}
public interface IHittable
{
    void Hit(HitArgs args);
}