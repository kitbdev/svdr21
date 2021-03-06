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
            interactionManager.CancelInteractorSelection(this);
            if (isArrowArmed)
            {
                float normPullAmount = Mathf.InverseLerp(releaseThreshold, 1f, pullAmount);
                VRDebug.Log("Arrow launched " + normPullAmount * 100 + "%", 3);
                currentArrow.ArrowLaunched(normPullAmount);
            } else
            {
                VRDebug.Log("Arrow dropped", 3);
                currentArrow.ArrowDropped();
            }
        }
        isArrowArmed = false;
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // force hand to drop

        // interactionManager.CancelInteractorSelection(args.interactable.selectingInteractor);
        interactionManager.CancelInteractableSelection(args.interactable);
        base.OnSelectEntering(args);
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentArrow = args.interactable as BaseArrow;
        VRDebug.Log("Arrow entered " + currentArrow.gameObject.name, 3);
        isArrowArmed = false;
        currentArrow.ArrowSet();
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        VRDebug.Log("Arrow exited", 3);
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
        return base.CanSelect(interactable) && CanHover(interactable) && interactable is BaseArrow;
    }
    public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride
    {
        // arrow will have smoother movement when with the bow
        get { return XRBaseInteractable.MovementType.Instantaneous; }
    }
    // allows socket to grab immediately
    public override bool requireSelectExclusive => false;
}
