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
    public ArrowMenu arrowMenu;
    public XRBaseInteractor leftHand;
    public XRBaseInteractor rightHand;
    public bool defaultToLeftHand = true;
    public XRBaseInteractor offHand { get; protected set; }

    private void Start()
    {
        SetBowHand(defaultToLeftHand);
        arrowMenu.ShowArrows();
    }
    [ContextMenu("Set bow hand")]
    void SetDefBow()
    {
        SetBowHand(defaultToLeftHand);
    }
    public void SetBowHand(bool left)
    {
        VRDebug.Log("Bow in " + (left ? "left" : "right") + " hand", debugContext: this);
        if (left)
        {
            if (rightHand.isPerformingManualInteraction)
            {
                rightHand.EndManualInteraction();
            }
            leftHand.StartManualInteraction(this);
            // interactionManager.ForceSelect(leftHand, this);
            offHand = rightHand;
        } else
        {
            if (leftHand.isPerformingManualInteraction)
            {
                leftHand.EndManualInteraction();
            }
            rightHand.StartManualInteraction(this);
            // interactionManager.ForceSelect(rightHand, this);
            offHand = leftHand;
        }
    }
    [ContextMenu("Drop bow")]
    public void DropBow()
    {
        if (rightHand.isPerformingManualInteraction)
        {
            rightHand.EndManualInteraction();
        }
        if (leftHand.isPerformingManualInteraction)
        {
            leftHand.EndManualInteraction();
        }
        interactionManager.CancelInteractableSelection(this);
        VRDebug.Log("Bow dropped!", debugContext: this);
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        VRDebug.Log("bow selected!");
        base.OnSelectEntered(args);
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        VRDebug.Log("bow deselected!");
        base.OnSelectExited(args);
    }
    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        // only force select
        return interactor is XRDirectInteractor;
    }
    private void Update()
    {
        Vector3 offhandLocal = transform.InverseTransformPoint(offHand.transform.position);
        float menuSwitchAt = 0.1f;
        if (arrowMenu.isOnRightSide && offhandLocal.x < -menuSwitchAt)
        {
            arrowMenu.SetSide(false);
        } else if (!arrowMenu.isOnRightSide && offhandLocal.x > menuSwitchAt)
        {
            arrowMenu.SetSide(true);
        }
    }
}
