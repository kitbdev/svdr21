using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Holds the arrow
/// </summary>
public class BowNotch : XRSocketInteractor
{
    [Header("Notch")]
    [Range(0, 1f)]
    public float releaseThreshold = 0.25f;
    public bool canGrabArrows = true;
    protected bool isArrowArmed;
    protected BaseArrow currentArrow;
    protected float pullAmount;
    public BowString bowString;
    XRBaseInteractor offHand;

    public void UpdatePull(float amount)
    {
        if (currentArrow == null)
        {
            return;
        }
        pullAmount = amount;
        if (amount >= releaseThreshold)
        {
            if (!isArrowArmed)
            {
                isArrowArmed = true;
                currentArrow.ArrowArmed();
            }
        } else if (isArrowArmed)
        {
            isArrowArmed = false;
            currentArrow.ArrowUnArmed();
        }
    }
    public void Grabbed()
    {
    }
    public void ReleaseArrow()
    {
        if (currentArrow != null)
        {
            var launchArrow = currentArrow;
            interactionManager.CancelInteractorSelection(this);
            if (isArrowArmed)
            {
                float normPullAmount = Mathf.InverseLerp(releaseThreshold, 1f, pullAmount);
                VRDebug.Log("Arrow launched " + normPullAmount * 100 + "%");
                launchArrow.ArrowLaunched(normPullAmount);
            } else
            {
                VRDebug.Log("Arrow dropped");
                launchArrow.ArrowDropped();
            }
        }
        isArrowArmed = false;
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // no need to force hand to drop, we are stealing the arrow anyway
        // want to force hand to interact with string, however
        // XRBaseInteractor hand = args.interactable.selectingInteractor;
        // //? && hand is Direct
        // if (hand != null)
        // {
        //     VRDebug.Log("Forcing hand string connection");
        //     interactionManager.ForceSelect(hand, bowString);
        // } else
        // {
        //     // VRDebug.Log("Grabbing Lose arrow!");
        // }
        base.OnSelectEntering(args);
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentArrow = args.interactable as BaseArrow;
        VRDebug.Log("Arrow '" + currentArrow.gameObject.name + "' notched");
        isArrowArmed = false;
        currentArrow.ArrowSet();
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        VRDebug.Log("Arrow exited");
        currentArrow.ArrowUnSet();
        currentArrow = null;
    }
    public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractor(updatePhase);
        // if (isSelected)
        // {
        //     if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        //     {

        //     }
        // }
    }
    public override bool CanSelect(XRBaseInteractable interactable)
    {
        // check hover for timing stuff - socket recycle time
        // prevents immediate regrabbing arrow
        // must be a base arrow interactable (or inherited)
        bool interactableArrow = interactable is BaseArrow && interactable.selectingInteractor != null;
        return base.CanSelect(interactable) && CanHover(interactable) && interactableArrow && canGrabArrows;
    }
    public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride
    {
        // arrow will have smoother movement when with the bow
        get { return XRBaseInteractable.MovementType.Instantaneous; }
    }
    // allows socket to grab immediately
    public override bool requireSelectExclusive => false;
}
