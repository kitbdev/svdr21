using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// All interactables in the level should be this or inherit
/// can be interacted with to do a generic action
/// may be toggleable
/// </summary>
public class LevelInteractable : MonoBehaviour
{
    public bool isToggle = false;
    public bool lockToOn = false;
    [ReadOnly] [SerializeField] protected bool isToggleOn = false;
    [ReadOnly] [SerializeField] protected bool isLocked = false;
    public AudioClip interactClip;
    public UnityEvent interactEvent;
    public float interactCooldown = 0.1f;
    protected float lastInteractTime = 0;


    protected AudioSource audioSource;// todo
    protected Animator anim;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        isToggleOn = false;
        lastInteractTime = 0;
    }
    [ContextMenu("Interact")]
    void EInteract()
    {
        Interact();
    }
    [ContextMenu("End Interact")]
    void EEndInteract()
    {
        EndInteract();
    }

    public virtual void Interact()
    {
        if (Time.time < lastInteractTime + interactCooldown)
        {
            VRDebug.Log(name + " interact in cooldown", default, this);
            return;
        }
        if (isToggle)
        {
            if (isToggleOn) return;
            isToggleOn = true;
            // isToggleOn = !isToggleOn;
            if (anim) anim.SetBool("Interact", isToggleOn);
        } else
        {
            if (anim)
            {
                // todo trigger instead?
                anim.SetBool("Interact", true);
            }

        }
        if (interactClip)
        {
            // todo
        }
        lastInteractTime = Time.time;
        interactEvent.Invoke();
    }
    public virtual void EndInteract()
    {
        if (Time.time < lastInteractTime + interactCooldown)
        {
            VRDebug.Log(name + " end interact in cooldown", default, this);
            return;
        }
        if (isToggle)
        {
            if (!isToggleOn) return;
            isToggleOn = false;
            if (anim) anim.SetBool("Interact", isToggleOn);
        }
        lastInteractTime = Time.time;
    }
}