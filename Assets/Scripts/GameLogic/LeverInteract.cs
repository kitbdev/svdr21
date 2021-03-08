using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

[RequireComponent(typeof(LevelInteractable))]
public class LeverInteract: XRBaseInteractable
{
    protected LevelInteractable interactable;
    protected override void Awake()
    {
        base.Awake();
        interactable = GetComponent<LevelInteractable>();
    }
    protected override void OnActivated(ActivateEventArgs args)
    {
        // todo determine when to trigger activate func
        base.OnActivated(args);
        interactable.Interact();
    }
}