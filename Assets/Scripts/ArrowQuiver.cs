using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArrowQuiver : XRBaseInteractable
{
    [Header("Quiver")]
    public GameObject arrowPrefab;

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        VRDebug.Log("Quiver hover new arrow");
        base.OnHoverEntered(args);
    }
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        VRDebug.Log("Quiver selling new arrow");
        base.OnSelectEntering(args);
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        VRDebug.Log("Quiver new arrow");
        base.OnSelectEntered(args);
        // spawn arrow
        Transform interT = args.interactor.transform;
        GameObject arrow = Instantiate(arrowPrefab, interT.position, interT.rotation);
        XRBaseInteractable arrInt = arrow.GetComponent<XRBaseInteractable>();
        // force select it
        interactionManager.ForceSelect(args.interactor, arrInt);
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        VRDebug.Log("Quiver desel");
    }
    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        return true;// base.IsSelectableBy(interactor) && interactor is XRDirectInteractor;
    }
}
