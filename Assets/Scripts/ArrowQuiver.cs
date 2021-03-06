using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArrowQuiver : XRBaseInteractable
{
    public GameObject arrowPrefab;
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // spawn arrow
        Transform interT = args.interactor.transform;
        GameObject arrow = Instantiate(arrowPrefab, interT.position, interT.rotation);
        XRBaseInteractable arrInt = arrow.GetComponent<XRBaseInteractable>();
        // force select it
        interactionManager.ForceSelect(args.interactor, arrInt);
    }
}
