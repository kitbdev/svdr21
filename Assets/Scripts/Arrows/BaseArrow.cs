using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Arrow Projectile
/// </summary>
[SelectionBase]
public class BaseArrow : XRGrabInteractable
{
    [Header("Arrow Settings")]
    public float launchForceMin = 2000;
    public float launchForceMax = 5000;
    public Transform tip;
    public LayerMask collisionMask = ~Physics.IgnoreRaycastLayer;

    protected Vector3 lastPos = Vector3.zero;
    protected bool launched = false;
    protected float launchPullAmount = 0;
    protected bool stopped = false;
    protected bool isSet = false;
    protected bool isArmed = false;

    protected bool inFlight => launched && !stopped;
    // protected BowNotch launcher;

    protected Collider col;
    protected Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        // col = GetComponent<Collider>();
        launched = false;
        stopped = false;
        SetPhysicsEnabled(false);
    }
    [ContextMenu("Set Defaults")]
    protected void SetDefaults()
    {
        interactionLayerMask = ~LayerMask.NameToLayer("Bow") & ~Physics.IgnoreRaycastLayer;
        colliders.Clear();
        colliders.Add(GetComponentInChildren<Collider>());
        movementType = MovementType.Instantaneous;
        retainTransformParent = false;
        tip = transform.Find("tip");
        attachTransform = transform.Find("attach");
        collisionMask = ~Physics.IgnoreRaycastLayer & ~LayerMask.NameToLayer("Bow") & ~LayerMask.NameToLayer("Arrow");
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (args.interactor is XRDirectInteractor)
        {
            // arrow was grabbed by player
            // VRDebug.Log("arrow grabbed");
            SetPhysicsEnabled(false);
            launched = false;
            stopped = false;
        }
        base.OnSelectEntering(args);
    }
    protected override void Drop()
    {
        if (!isSet)
        {
            // Debug.Log("dropping");
            base.Drop();
            SetPhysicsEnabled(true);
        }
    }
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (inFlight)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {

            } else if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                SetDirToVel();
            } else if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Late)
            {
                CheckHit();
            }
        }
    }
    protected void SetDirToVel()
    {
        if (rb.velocity.z > 0.5f)
        {
            transform.forward = rb.velocity.normalized;
        }
    }

    protected void CheckHit()
    {
        if (inFlight)
        {
            float dist = Vector3.Distance(tip.transform.position, lastPos);
            Vector3 dir = tip.transform.position - lastPos;
            Debug.DrawRay(lastPos, dir);
            if (Physics.SphereCast(lastPos, 0.05f, dir, out var hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                // hit something
                VRDebug.Log("Arrow hit " + hit.collider.name, 5, this);
                stopped = true;
                // remove physics
                SetPhysicsEnabled(false);
                // child arrow
                transform.SetParent(hit.collider.transform);
                // check hittable
                if (hit.transform.gameObject.TryGetComponent<IHittable>(out var hittable))
                {
                    HitArgs args = new HitArgs();
                    args.isDirect = true;
                    SetHitArgs(ref args);
                    hittable.Hit(args);
                    ArrowHit(hit);
                }
            }
            lastPos = tip.transform.position;
        }
    }
    protected void SetPhysicsEnabled(bool enabled)
    {
        // VRDebug.Log("arrow phys " + enabled, -1);
        rb.isKinematic = !enabled;
        rb.useGravity = enabled;
    }
    protected virtual void SetHitArgs(ref HitArgs hitargs)
    {

    }

    protected void LaunchForce()
    {
        SetPhysicsEnabled(true);
        float forceAmount = Mathf.Lerp(launchForceMin, launchForceMax, launchPullAmount);
        VRDebug.Log("Launching at " + forceAmount + " force", debugContext: this);
        rb.AddForce(transform.forward * forceAmount, ForceMode.Impulse);
    }

    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        bool isGrabbing = interactor is XRDirectInteractor && !isSet;
        bool isNotching = interactor is BowNotch;
        return base.IsSelectableBy(interactor) && (isGrabbing || isNotching);
    }
    // private void OnCollisionEnter(Collision other)
    // {
    // VRDebug.Log(other.collider.name + " arrow col", -1, other.collider);
    // }

    /// <summary>
    /// the arrow was set in the bow
    /// </summary>
    public virtual void ArrowSet()
    {
        isSet = true;
    }
    /// <summary>
    /// the arrow was taken out of the bow
    /// </summary>
    public virtual void ArrowUnSet()
    {
        isSet = false;
    }
    /// <summary>
    /// The arrow was pulled past the launch threshold
    /// </summary>
    public virtual void ArrowArmed()
    {
        isArmed = true;
    }
    /// <summary>
    /// the arrow was put below the launch threshold after being armed
    /// </summary>
    public virtual void ArrowUnArmed()
    {
        isArmed = false;
    }
    /// <summary>
    /// the arrow is being launched
    /// </summary>
    public virtual void PreviewLaunchForce(float pullAmount)
    {
    }
    /// <summary>
    /// the arrow is being launched
    /// </summary>
    public virtual void ArrowLaunched(float pullAmount)
    {
        launched = true;
        transform.position = tip.transform.position;
        launchPullAmount = pullAmount;
        LaunchForce();
    }
    /// <summary>
    /// the arrow was not launched, but dropped
    /// prepare for destruction
    /// </summary>
    public virtual void ArrowDropped()
    {
        Destroy(gameObject);
    }
    /// <summary>
    /// the alt button was hit while the arrow was armed
    /// </summary>
    public virtual void ArrowAlt()
    {
    }
    /// <summary>
    /// the arrow hit its target
    /// </summary>
    public virtual void ArrowHit(RaycastHit hit)
    {
    }
}