using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleArrow : BaseArrowLogic
{
    [Space]
    [SerializeField] float verticalityThreshold = 0.6f;
    [SerializeField] float wallOffsetDist = 0.5f;
    [SerializeField] float tpDropDist = 2f;
    [SerializeField] float tpMinHeight = 1.55f;
    [SerializeField] float tpMinWidth = 0.1f;
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
        // VRDebug.Log("tp hit");
        if (TryTeleportLocation(hit))
        {
            return;
        } else
        {
            // raycast down to find a good spot
            Vector3 startPos = hit.point + hit.normal * wallOffsetDist;
            Debug.DrawRay(startPos, Vector3.down * tpDropDist, Color.magenta, 2);
            if (Physics.Raycast(startPos, Vector3.down, out var hitDown, tpDropDist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                if (TryTeleportLocation(hitDown))
                {
                    return;
                }
            }
        }
        // failed to teleport
        VRDebug.Log("Failed to teleport");
    }
    protected bool TryTeleportLocation(RaycastHit hit)
    {
        // check if surface is flat
        float hitDotUp = Vector3.Dot(hit.normal, Vector3.up);
        if (hitDotUp >= verticalityThreshold)
        {
            // check if area is wide enough
            bool areaSizeOk = false;
            float checkDist = tpMinWidth;
            var cols = Physics.OverlapSphere(hit.point + (checkDist + 0.01f) * Vector3.up, checkDist, collisionMask, QueryTriggerInteraction.Ignore);
            if (cols.Length == 0)
            {
                areaSizeOk = true;
            } else
            {
                Debug.Log("TP blocked: Cols " + cols.Length);
                foreach (var col in cols)
                {
                    Debug.Log("Col " + col.name);
                }
            }
            if (areaSizeOk)
            {
                // height check
                Debug.DrawRay(hit.point, Vector3.up * tpMinHeight, Color.magenta, 2);
                //? different mask - playercol
                if (!Physics.SphereCast(hit.point, tpMinWidth, Vector3.up, out var hitheight, tpMinHeight, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    TeleportTo(hit.point);
                    return true;
                } else
                {
                    VRDebug.Log("TP blocked by height");
                }
            }
        }
        return false;
    }
    public void TeleportTo(Vector3 pos)
    {
        // todo cool effect

        VRDebug.Log("teleporting to " + pos + "...");
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