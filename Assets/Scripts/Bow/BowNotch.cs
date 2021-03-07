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
    public ArrowInteractable currentArrow { get; protected set; }
    protected float pullAmount;
    public BowString bowString;
    public Bow bow;

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
                bow.ArrowArmed();
            }
        } else if (isArrowArmed)
        {
            isArrowArmed = false;
            currentArrow.ArrowUnArmed();
            bow.ArrowUnArmed();
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
            // disconnect with arrow
            interactionManager.CancelInteractorSelection(this);
            // launch or drop it
            if (isArrowArmed)
            {
                float normPullAmount = Mathf.InverseLerp(releaseThreshold, 1f, pullAmount);
                VRDebug.Log("Arrow launched " + normPullAmount * 100 + "%");
                launchArrow.ArrowLaunched(normPullAmount);
                bow.ArrowLaunched(normPullAmount);
            } else
            {
                VRDebug.Log("Arrow dropped");
                launchArrow.ArrowDropped();
                bow.ArrowDropped();
            }
        }
        bow.ArrowReleased();
        isArrowArmed = false;
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // no need to force hand to drop, we are stealing the arrow anyway

        base.OnSelectEntering(args);
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // need to force hand to go on the string otherwise it could select something else
        // VRDebug.Log("Forcing hand string connection");
        // if (bow.offHand.selectTarget != null && bow.offHand.selectTarget != bowString)
        // {
        //     interactionManager.CancelInteractorSelection(bow.offHand);
        // }
        // interactionManager.SelectEnter(bow.offHand, bowString);
        // interactionManager.ForceSelect(bow.offHand, bowString);

        if (!(args.interactable is ArrowInteractable))
        {
            VRDebug.Log("wrong interaction " + args.interactable.name, 5);
        }

        // get arrow
        currentArrow = args.interactable as ArrowInteractable;
        VRDebug.Log("Arrow '" + currentArrow.gameObject.name + "' notched");
        isArrowArmed = false;
        currentArrow.ArrowSet();
        currentArrow.SetBow(bow);
        bow.ArrowSet();
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        VRDebug.Log("Arrow exited");
        currentArrow.ArrowUnSet();
        bow.ArrowUnSet();
        currentArrow = null;
    }
    public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractor(updatePhase);
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isArrowArmed && currentArrow)
            {
                float normPullAmount = Mathf.InverseLerp(releaseThreshold, 1f, pullAmount);
                currentArrow.PreviewLaunchForce(normPullAmount);
                bow.PreviewLaunchForce(normPullAmount);
            }
        }
    }
    public override bool CanSelect(XRBaseInteractable interactable)
    {
        // check hover for timing stuff - socket recycle time
        // prevents immediate regrabbing arrow
        // must be a base arrow interactable (or inherited) and must be held
        bool interactableArrow = interactable is ArrowInteractable && interactable.selectingInteractor != null;
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
