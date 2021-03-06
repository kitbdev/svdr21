using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// handles two hand rotation stuff
/// </summary>
public class Bow : XRGrabInteractable
{
    [Space]
    public BowString BowString;
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
    }
    // todo no deselect unless switching hands
}
