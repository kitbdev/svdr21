using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Arrow Projectile logic
/// </summary>
[SelectionBase]
public class BaseArrowLogic : MonoBehaviour
{
    [HideInInspector]
    public string typeName = "Arrow";
    [Header("Arrow Settings")]
    public float launchForceMin = 5;
    public float launchForceMax = 20;
    public Transform tip;
    public LayerMask collisionMask = ~Physics.IgnoreRaycastLayer;
    public float flightDestroyDelay = 100;
    public float groundDestroyDelay = 60 * 10;
    public float damage = 0;

    [Header("Dynamic")]
    public bool launched = false;
    public bool stopped = false;
    public bool isSet = false;
    public bool isArmed = false;
    protected Vector3 lastPos = Vector3.zero;
    protected float launchPullAmount = 0;
    protected float launchTime = 0;
    protected float groundHitTime = 0;

    protected bool inFlight => launched && !stopped;
    protected Bow launchBow;
    protected ArrowInteractable arrowInteractable;

    protected Rigidbody rb;
    // protected Collider col;

    protected virtual void Awake()
    {
        typeName = "Base Arrow";
        rb = GetComponent<Rigidbody>();
        arrowInteractable = GetComponent<ArrowInteractable>();
        // col = GetComponent<Collider>();
        launched = false;
        stopped = false;
        SetPhysicsEnabled(false);
    }
    private void Reset()
    {
        launchForceMin = 5;
        launchForceMax = 20;
        tip = transform.Find("tip");
        collisionMask = ~Physics.IgnoreRaycastLayer & ~LayerMask.NameToLayer("Bow") & ~LayerMask.NameToLayer("Arrow");
    }
    protected virtual void Update()
    {
        if (inFlight && Time.time > launchTime + flightDestroyDelay)
        {
            // VRDebug.Log("Arrow timeout destroyed in air");
            Destroy(gameObject);
        } else if (stopped && Time.time > launchTime + groundDestroyDelay)
        {
            // VRDebug.Log("Arrow timeout destroyed on ground");
            Destroy(gameObject);
        }
    }
    protected virtual void FixedUpdate()
    {
        if (inFlight)
        {
            SetDirToVel();

        }
    }
    protected virtual void LateUpdate()
    {
        if (inFlight)
        {
            CheckHit();
        }
    }
    protected virtual void SetDirToVel()
    {
        if (rb.velocity.z > 0.5f)
        {
            transform.forward = rb.velocity.normalized;
        }
    }

    protected virtual void CheckHit()
    {
        if (inFlight)
        {
            float dist = Vector3.Distance(tip.transform.position, lastPos);
            Vector3 dir = tip.transform.position - lastPos;
            Debug.DrawRay(lastPos, dir);
            if (Physics.SphereCast(lastPos, 0.05f, dir, out var hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                // todo piercing keep going for range or until hit a non hittable
                // hit something
                VRDebug.Log("Arrow hit " + hit.collider.name, 5, this);
                stopped = true;
                // remove physics
                SetPhysicsEnabled(false);
                // child arrow to new transform for correct scale
                Transform newParent = new GameObject(name + " stick").transform;
                newParent.SetParent(hit.collider.transform, true);
                newParent.position = hit.point;
                newParent.rotation = Quaternion.identity;
                transform.SetParent(newParent, true);
                // transform.SetParent(hit.collider.transform, true);
                groundHitTime = Time.time;
                ArrowHit(hit);
                // check hittable
                if (hit.transform.gameObject.TryGetComponent<IHittable>(out var hittable))
                {
                    HitArgs args = new HitArgs();
                    args.isDirect = true;
                    args.damage = damage;
                    args.attacker = launchBow.ownerName;
                    SetHitArgs(ref args);
                    hittable.Hit(args);
                }
            }
            lastPos = tip.transform.position;
        }
    }
    public virtual void SetPhysicsEnabled(bool enabled)
    {
        // VRDebug.Log("arrow phys " + enabled, -1);
        rb.isKinematic = !enabled;
        rb.useGravity = enabled;
    }
    protected virtual void SetHitArgs(ref HitArgs hitargs)
    {

    }

    public virtual float GetLaunchForce(float pullAmount)
    {
        return Mathf.Lerp(launchForceMin, launchForceMax, pullAmount);
    }
    protected virtual void LaunchForce()
    {
        SetPhysicsEnabled(true);
        float forceAmount = GetLaunchForce(launchPullAmount);
        VRDebug.Log("Launching at " + forceAmount + " force", debugContext: this);
        rb.AddForce(transform.forward * forceAmount, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        VRDebug.Log(other.collider.name + " arrow bump", -1, other.collider);
    }

    public virtual void SetBow(Bow bow)
    {
        launchBow = bow;
    }
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
    /// the arrow was grabbed by the player
    /// </summary>
    public virtual void ArrowGrabbed()
    {
        isArmed = false;
        // VRDebug.Log("arrow grabbed");
        SetPhysicsEnabled(false);
        launched = false;
        stopped = false;
    }
    /// <summary>
    /// the arrow is being launched
    /// </summary>
    public virtual void PreviewLaunchForce(float pullAmount)
    {
        if (launchBow)
        {
            float lf = GetLaunchForce(pullAmount);
            VRDebug.LogFrame("PreviewForce " + lf);
            launchBow.PreviewLaunch(lf);
        }
    }
    /// <summary>
    /// the arrow is being launched
    /// </summary>
    public virtual void ArrowLaunched(float pullAmount)
    {
        launched = true;
        transform.position = tip.transform.position;
        launchPullAmount = pullAmount;
        launchTime = Time.time;
        LaunchForce();
    }
    /// <summary>
    /// the arrow was not launched, but dropped
    /// prepare for destruction
    /// </summary>
    public virtual void ArrowDropped()
    {
        VRDebug.Log("Arrow dropped ", default, gameObject);
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