using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Door : LevelInteractable
{

    /*
    Door features
    closed for general level flow - need to get a key to open
    when closed, always lock
    closed for ?
    */

    public bool startsClosed = false;
    // gates are locked until a 'key' is gotten
    public bool isGate = false;
    // public bool isKeyLocked = false;
    public GameObject block;

    protected override void Awake()
    {
        base.Awake();
        interactCooldown = -1;
    }
    private void Start()
    {
        if (startsClosed)
        {
            EndInteract();
        } else
        {
            Interact();
        }
    }
    public void MakeIntoGate()
    {
        isGate = true;
        // todo
        if (isGate)
        {
            startsClosed = true;
            EndInteract();
        }
    }
    public void StartsClosed()
    {
        startsClosed = true;
    }
    // public void OpenDoor() {

    // }
    // public void CloseDoor() {

    // }
    // public void LockDoor() {
    //     CloseDoor();
    //     // lock
    // }
    // public void UnLockDoor() {

    // }
    public override void Interact()
    {
        base.Interact();
        // open
        block.SetActive(false);
        // todo anim
    }
    public override void EndInteract()
    {
        base.EndInteract();
        // close
        block.SetActive(true);
        // todo
    }
}