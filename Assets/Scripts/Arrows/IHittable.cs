using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitArgs : Object
{
    public float damage;
    public bool isDirect;
    public string attacker;
    public Vector3 point;
    public Vector3 velocity;
    public GameObject hit;
}
public interface IHittable
{
    void Hit(HitArgs args);
}