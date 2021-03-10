using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// Helps play animations from events
/// </summary>
public class PlayAnim : MonoBehaviour
{
    public string[] animNames = new string[0];
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void TriggerAnim(int index = 0)
    {
        anim.SetTrigger(animNames[index]);
    }
    public void SetBoolAnimTrue(int index = 0)
    {
        anim.SetBool(animNames[index], true);
    }
    public void SetBoolAnimFalse(int index = 0)
    {
        anim.SetBool(animNames[index], false);
    }
    public void ToggleBoolAnim(int index = 0)
    {
        anim.SetBool(animNames[index], !anim.GetBool(animNames[index]));
    }
}