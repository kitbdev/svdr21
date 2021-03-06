using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// handles two hand rotation stuff
/// </summary>
[SelectionBase]
public class Bow : XRGrabInteractable
{
    [Space]
    public BowString bowString;
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
    }
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        // todo no deselect unless switching hands
        base.OnSelectExiting(args);
    }
    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        return base.IsSelectableBy(interactor) && interactor is XRDirectInteractor;
    }
}
