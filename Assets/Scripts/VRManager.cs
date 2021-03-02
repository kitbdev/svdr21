using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class VRManager : Singleton<VRManager>
{

    // 2d mode switch
    public bool isVREnabled;// { get; private set; }

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
        // todo check if vr is enabled

    }

    void Start()
    {

    }

    void Update()
    {

    }
}
