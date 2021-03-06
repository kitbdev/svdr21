using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BowString : XRBaseInteractable
{
    public Transform stringGrab;
    public LineRenderer bowstringLR;

    private void LateUpdate()
    {
        if (stringGrab.hasChanged)
        {
            UpdateLine();
        }
    }
    void UpdateLine()
    {
        Vector3 centerLoc = bowstringLR.transform.InverseTransformPoint(stringGrab.position);
        bowstringLR.SetPosition(1, centerLoc);

    }
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (isSelected)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // check for a pull

            }
        }
    }
}
