using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Management;

[DefaultExecutionOrder(-100)]
public class VRManager : Singleton<VRManager>
{

    // disables on awake
    public bool disableVR = false;

    public bool isVRReady { get; protected set; }
    float vrTimeout = 2;

    public VRSpaceMode vrSpaceMode;
    public enum VRSpaceMode
    {
        STANDING,
        SITTING,
        ROOM,
        FLAT, // 2d, non vr
    }

    protected override void Awake()
    {
        base.Awake();
        isVRReady = false;

        // XRSettings.enabled = !disableVR;
        if (disableVR)
        {
            SetVREnabled(false);
            vrSpaceMode = VRSpaceMode.FLAT;
        }
    }
    private void OnEnable()
    {
        XRDevice.deviceLoaded += OnDeviceLoad;
    }
    private void OnDisable()
    {
        XRDevice.deviceLoaded -= OnDeviceLoad;
    }

    void OnDeviceLoad(string name)
    {
        VRDebug.Log("Device loaded " + name, 2);
        isVRReady = true;
    }
    void OnBoundaryChange()
    {

    }


    IEnumerator Start()
    {
        Debug.Log("start");
        yield return new WaitForSecondsRealtime(vrTimeout);
        if (!disableVR && !XRSettings.isDeviceActive)
        {
            Debug.LogWarning("No VR detected!");
        }
    }

    /// <summary>
    /// is VR Enabled, not necessarily ready
    /// </summary>
    public bool IsVREnabled()
    {
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);
        foreach (var displaysubsys in displaySubsystems)
        {
            if (displaysubsys.running)
            {
                return true;
            }
        }
        return false;
    }
    public void SetVREnabled(bool enabled)
    {
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);
        foreach (var displaysubsys in displaySubsystems)
        {
            if (enabled && !displaysubsys.running)
            {
                displaysubsys.Start();
            } else if (!enabled && displaysubsys.running)
            {
                displaysubsys.Stop();
            }
        }
    }
    void GetTrackingMode()
    {
        // List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        // SubsystemManager.GetInstances<XRInputSubsystem>(subsystems);
        // foreach (var inputsubsys in subsystems)
        // {
        //     inputsubsys.GetTrackingOriginMode();
        //     TrackingOriginModeFlags.
        // }
    }

    public void SetSpaceMode(VRSpaceMode spaceMode)
    {
        vrSpaceMode = spaceMode;
    }
}
