using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Information about a IHittable Hit
/// </summary>
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
/// <summary>
/// Interface for game objects that can be hit
/// </summary>
public interface IHittable
{
    void Hit(HitArgs args);
}