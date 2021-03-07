using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleArrow : BaseArrow
{
    public float verticalityThreshold = 0.6f;
    public override void ArrowHit(RaycastHit hit)
    {
        base.ArrowHit(hit);
        float hitDotUp = Vector3.Dot(hit.normal, Vector3.up);
        if (hitDotUp >= verticalityThreshold)
        {
            TeleportTo(hit.point);
            return;
        } else
        {
            // raycast down to find a good spot
            Vector3 startPos = hit.point + hit.normal * 0.5f;
            float maxDist = 1.8f;
            if (Physics.Raycast(startPos, Vector3.down, out var hit1, maxDist, collisionMask))
            {
                // todo also check if area is wide enough
                hitDotUp = Vector3.Dot(hit.normal, Vector3.up);
                if (hitDotUp >= verticalityThreshold)
                {
                    TeleportTo(hit.point);
                    return;
                }
            }
        }
        // failed to teleport
        VRDebug.Log("Failed to teleport");
    }
    public void TeleportTo(Vector3 pos)
    {
        // todo cool effect
    }
    public override void PreviewLaunchForce(float pullAmount)
    {
        base.PreviewLaunchForce(pullAmount);
        float forceAmount = Mathf.Lerp(launchForceMin, launchForceMax, launchPullAmount);
    }
}