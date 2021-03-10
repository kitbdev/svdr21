using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Door : LevelInteractable
{

    public bool startsClosed = false;
    // gates are locked until a 'key' is gotten
    public bool isGate = false;
    // public bool isKeyLocked = false;

    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        if (startsClosed)
        {
            EndInteract();
        }
    }
    public void MakeIntoGate()
    {
        isGate = true;
    }
    public void StartsClosed()
    {
        startsClosed = true;
    }
    public override void Interact()
    {
        base.Interact();
        // open
        // todo 
    }
    public override void EndInteract()
    {
        base.EndInteract();
        // close
        // todo
    }
}