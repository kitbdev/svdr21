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
    [Header("Bow")]
    public BowString bowString;
    public ArrowMenu arrowMenu;
    public ProjectileLine line;
    [SerializeField]
    bool m_showLine = false;
    public bool showLine { get { return m_showLine; } set { m_showLine = value; line.lineActive = value; } }
    [Header("Hands")]
    public XRBaseInteractor leftHand;
    public XRBaseInteractor rightHand;
    public bool defaultToLeftHand = true;
    public XRBaseInteractor bowHand { get; protected set; }
    public XRBaseInteractor offHand { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        if (!arrowMenu) arrowMenu = GetComponentInChildren<ArrowMenu>();
        if (!line) line = GetComponentInChildren<ProjectileLine>();
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
    public void PreviewLaunch(float val)
    {
        
    }
    public void Launched()
    {
        arrowMenu.ShowArrows();
    }
}
