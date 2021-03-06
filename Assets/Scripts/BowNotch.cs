using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BowNotch : XRSocketInteractor
{
    [Header("Notch")]
    [Range(0, 1f)]
    public float releaseThreshold = 0.5f;
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
}
