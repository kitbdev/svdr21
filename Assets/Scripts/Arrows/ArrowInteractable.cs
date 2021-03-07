using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Arrow grab interactable
/// handles interfacing with bow and XR
/// </summary>
[SelectionBase]
public class ArrowInteractable : XRGrabInteractable
{
    protected BaseArrowLogic arrowLogic;

    protected override void Awake()
    {
        base.Awake();
        arrowLogic = GetComponent<BaseArrowLogic>();
    }
    protected override void Reset()
    {
        base.Reset();
        SetDefaults();
    }
    [ContextMenu("Set Defaults")]
    protected void SetDefaults()
    {
        interactionLayerMask = ~LayerMask.NameToLayer("Bow") & ~Physics.IgnoreRaycastLayer;
        colliders.Clear();
        colliders.Add(GetComponentInChildren<Collider>());
        movementType = MovementType.Instantaneous;
        retainTransformParent = false;
        attachTransform = transform.Find("attach");
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (args.interactor is XRDirectInteractor)
        {
            // arrow was grabbed by player
            arrowLogic.ArrowGrabbed();
        }
        base.OnSelectEntering(args);
    }
    protected override void Drop()
    {
        if (!arrowLogic.isSet)
        {
            // Debug.Log("dropping");
            base.Drop();
            arrowLogic.SetPhysicsEnabled(true);
        }
    }

    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        bool isGrabbing = interactor is XRDirectInteractor && !arrowLogic.isSet;
        bool isNotching = interactor is BowNotch;
        return base.IsSelectableBy(interactor) && (isGrabbing || isNotching);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        interactionManager.UnregisterInteractable(this);
    }

    /// <summary>
    /// the arrow was set in the bow
    /// </summary>
    public void ArrowSet()
    {
        arrowLogic.ArrowSet();
    }
    /// <summary>
    /// the arrow was taken out of the bow
    /// </summary>
    public void ArrowUnSet()
    {
        arrowLogic.ArrowUnSet();
    }
    /// <summary>
    /// The arrow was pulled past the launch threshold
    /// </summary>
    public void ArrowArmed()
    {
        arrowLogic.ArrowArmed();
    }
    /// <summary>
    /// the arrow was put below the launch threshold after being armed
    /// </summary>
    public void ArrowUnArmed()
    {
        arrowLogic.ArrowUnArmed();
    }
    /// <summary>
    /// the arrow is being launched
    /// </summary>
    public void PreviewLaunchForce(float pullAmount)
    {
        arrowLogic.PreviewLaunchForce(pullAmount);
    }
    /// <summary>
    /// the arrow is being launched
    /// </summary>
    public void ArrowLaunched(float pullAmount)
    {
        arrowLogic.ArrowLaunched(pullAmount);
    }
    /// <summary>
    /// the arrow was not launched, but dropped
    /// prepare for destruction
    /// </summary>
    public void ArrowDropped()
    {
        arrowLogic.ArrowDropped();
    }
    /// <summary>
    /// the alt button was hit while the arrow was armed
    /// </summary>
    public void ArrowAlt()
    {
        arrowLogic.ArrowAlt();
    }
    /// <summary>
    /// the arrow hit its target
    /// </summary>
    public void ArrowHit(RaycastHit hit)
    {
        arrowLogic.ArrowHit(hit);
    }
}