using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleArrow : BaseArrowLogic
{
    [Space]
    [SerializeField] float verticalityThreshold = 0.6f;
    [SerializeField] float wallOffsetdist = 0.5f;
    /// <summary>
    /// How to orient the rig after teleportation.
    /// </summary>
    [Tooltip("How to orient the rig after teleportation." +
        "\nSet to:" +
        "\n\nWorld Space Up to stay oriented according to the world space up vector." +
        "\n\nSet to Target Up to orient according to the target BaseTeleportationInteractable Transform's up vector." +
        "\n\nSet to Target Up And Forward to orient according to the target BaseTeleportationInteractable Transform's rotation." +
        "\n\nSet to None to maintain the same orientation before and after teleporting.")]
    public MatchOrientation matchOrientation = MatchOrientation.WorldSpaceUp;
    /// <summary>
    /// The teleportation provider that this teleportation interactable will communicate teleport requests to.
    /// If no teleportation provider is configured, will attempt to find a teleportation provider during Awake.
    /// </summary>
    protected TeleportationProvider teleportationProvider;


    protected override void Awake()
    {
        base.Awake();
        typeName = "Teleport Arrow";
        if (teleportationProvider == null)
        {
            teleportationProvider = FindObjectOfType<TeleportationProvider>();
        }
    }
    public override void ArrowHit(RaycastHit hit)
    {
        base.ArrowHit(hit);
        VRDebug.Log("tp hit");
        if (TryTeleportLocation(hit))
        {
            return;
        } else
        {
            // raycast down to find a good spot
            Vector3 startPos = hit.point + hit.normal * wallOffsetdist;
            float maxDist = 1.8f;
            if (Physics.Raycast(startPos, Vector3.down, out var hit1, maxDist, collisionMask))
            {
                if (TryTeleportLocation(hit))
                {
                    return;
                } else
                {
                    // todo try move away from walls?
                }
            }
        }
        // failed to teleport
        VRDebug.Log("Failed to teleport");
    }
    protected bool TryTeleportLocation(RaycastHit hit)
    {
        // todo also check if area is wide enough
        float hitDotUp = Vector3.Dot(hit.normal, Vector3.up);
        if (hitDotUp >= verticalityThreshold)
        {
            TeleportTo(hit.point);
            return true;
        }
        return false;
    }
    public void TeleportTo(Vector3 pos)
    {
        // todo cool effect
        
        VRDebug.Log("teleporting...");
        // world space up doesnt matter
        Quaternion rot = Quaternion.identity;
        var tr = new TeleportRequest {
            matchOrientation = matchOrientation,
            requestTime = Time.time,
            destinationPosition = pos,
            destinationRotation = rot,
        };
        teleportationProvider.QueueTeleportRequest(tr);
    }
    public override void PreviewLaunchForce(float pullAmount)
    {
        base.PreviewLaunchForce(pullAmount);
        float forceAmount = Mathf.Lerp(launchForceMin, launchForceMax, launchPullAmount);
    }
}