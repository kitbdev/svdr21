using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Held by hand to pull back string
/// </summary>
public class BowString : XRBaseInteractable
{
    [Header("BowString")]
    public Transform stringGrab;
    public LineRenderer bowstringLR;
    public Transform startPullT;
    public Transform endPullT;

    protected float pullAmount = 0;
    public float PullAmount => pullAmount;

    public BowNotch bowNotch;
    XRBaseInteractor pullingInteractor;

    // private void LateUpdate()
    // {
    //     if (stringGrab.hasChanged)
    //     {
    //         UpdateLine();
    //     }
    // }
    protected override void Awake()
    {
        if (!bowNotch) bowNotch = GetComponent<BowNotch>();
    }
    void Start()
    {
        pullAmount = -1;
        SetPullAmount(0);
    }
    void UpdateLine()
    {
        Vector3 centerLoc = bowstringLR.transform.InverseTransformPoint(stringGrab.position);
        bowstringLR.SetPosition(1, centerLoc);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        VRDebug.LogTemp("String Grabbed");
        // bowstring grabbed by player
        base.OnSelectEntered(args);
        pullingInteractor = args.interactor;
        bowNotch.Grabbed();
        UpdateLine();
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        VRDebug.LogTemp("String released");
        // bowstring released by player
        base.OnSelectExited(args);
        bowNotch.ReleaseArrow();
        SetPullAmount(0);
        pullingInteractor = null;
        // todo snap animation?
    }
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (isSelected)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // check pull amount
                Vector3 targDir = endPullT.position - startPullT.position;
                float maxLength = targDir.magnitude;
                targDir.Normalize();

                Vector3 pullDir = pullingInteractor.transform.position - startPullT.position;
                float nPull = Vector3.Dot(pullDir, targDir) / maxLength;
                nPull = Mathf.Clamp01(nPull);
                SetPullAmount(nPull);
            }
        }
    }
    void SetPullAmount(float value)
    {
        // only if value is different
        if (value != pullAmount)
        {
            pullAmount = value;
            bowNotch.UpdatePull(pullAmount);

            // update string postion
            Vector3 npos = Vector3.Lerp(startPullT.position, endPullT.position, pullAmount);
            transform.position = npos;
            // stringGrab.position = npos;
            UpdateLine();
        }
    }
    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        // only be selected by hand's direct interactor
        return base.IsSelectableBy(interactor) && interactor is XRDirectInteractor;
    }
    private void OnDrawGizmosSelected()
    {
        if (startPullT && endPullT)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPullT.position, endPullT.position);
            Gizmos.color = Color.white;
        }
    }
}
