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

    public void UpdatePull(float amount)
    {
        if (currentArrow == null)
        {
            return;
        }
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
        if (isArrowArmed)
        {
            Debug.Log("Arrow launched");
            currentArrow.ArrowLaunched();
        } else
        {
            Debug.Log("Arrow dropped");
            currentArrow.ArrowDropped();
        }
        isArrowArmed = false;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentArrow = args.interactable as BaseArrow;
        Debug.Log("Arrow entered " + currentArrow.gameObject.name);
        isArrowArmed = false;
        currentArrow.ArrowSet();
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        Debug.Log("Arrow exited");
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
