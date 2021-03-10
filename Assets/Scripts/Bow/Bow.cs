using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Bow xr interactable
/// grabbed in either hand
/// manages arrow menu, bow notch, bow string, and line all together
/// </summary>
[SelectionBase]
public class Bow : XRGrabInteractable
{
    [Header("Bow")]
    public string ownerName = GameManager.PlayerTag;
    public BowString bowString;
    public BowNotch bowNotch;
    public ArrowMenu arrowMenu;
    public ProjectileLine line;
    [SerializeField]
    bool m_showLine = false;
    public bool showLine { get { return m_showLine; } set { m_showLine = value; line.lineActive = value; } }
    [Header("Hands")]
    public XRBaseInteractor leftHand;
    public XRBaseInteractor rightHand;
    public bool defaultToLeftHand = true;
    [ReadOnly] public bool primaryLeftHand = true;
    public XRBaseInteractor bowHand { get; protected set; }
    public XRBaseInteractor offHand { get; protected set; }
    public bool debugLog = false;

    protected override void Awake()
    {
        base.Awake();
        if (!arrowMenu) arrowMenu = GetComponentInChildren<ArrowMenu>();
        if (!line) line = GetComponentInChildren<ProjectileLine>();
        if (!bowNotch) bowNotch = GetComponentInChildren<BowNotch>();
        if (!bowString) bowString = GetComponentInChildren<BowString>();
    }
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
        primaryLeftHand = left;
        if (left)
        {
            bowHand = leftHand;
            offHand = rightHand;
        } else
        {
            bowHand = rightHand;
            offHand = leftHand;
        }
        if (offHand.isPerformingManualInteraction)
        {
            offHand.EndManualInteraction();
        }
        bowHand.StartManualInteraction(this);
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
    // protected override void OnSelectEntered(SelectEnterEventArgs args)
    // {
    //     VRDebug.Log("bow selected!");
    //     base.OnSelectEntered(args);
    // }
    // protected override void OnSelectExited(SelectExitEventArgs args)
    // {
    //     VRDebug.Log("bow deselected!");
    //     base.OnSelectExited(args);
    // }
    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        // only force select
        return interactor is XRDirectInteractor && interactor == bowHand;
    }
    private void Update()
    {
        Vector3 offhandLocal = transform.InverseTransformPoint(offHand.transform.position);
        float menuSwitchAt = 0.02f;
        if (arrowMenu.isOnRightSide && offhandLocal.x < -menuSwitchAt)
        {
            arrowMenu.SetSide(false);
        } else if (!arrowMenu.isOnRightSide && offhandLocal.x > menuSwitchAt)
        {
            arrowMenu.SetSide(true);
        }
        // line.projectileVelocity = 
    }
    public void PreviewLaunch(float launchForce)
    {
        line.projectileVelocity = launchForce;
    }
    /// <summary>
    /// arrow was launched or dropped
    /// </summary>
    public void ArrowReleased()
    {
        arrowMenu.ShowArrows();
    }
    /// <summary>
    /// the arrow was set in the bow
    /// </summary>
    public virtual void ArrowSet()
    {
        showLine = false;
    }
    /// <summary>
    /// the arrow was taken out of the bow
    /// </summary>
    public virtual void ArrowUnSet()
    {
    }
    /// <summary>
    /// The arrow was pulled past the launch threshold
    /// </summary>
    public virtual void ArrowArmed()
    {
        showLine = true;
    }
    /// <summary>
    /// the arrow was put below the launch threshold after being armed
    /// </summary>
    public virtual void ArrowUnArmed()
    {
        // ? detach line
        showLine = false;
    }
    /// <summary>
    /// an arrow was grabbed by the player
    /// </summary>
    public virtual void ArrowGrabbed()
    {
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
    }
    /// <summary>
    /// the arrow was not launched, but dropped
    /// prepare for destruction
    /// </summary>
    public virtual void ArrowDropped()
    {
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
