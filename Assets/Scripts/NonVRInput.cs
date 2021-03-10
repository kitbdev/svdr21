using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[DefaultExecutionOrder(40)]
public class NonVRInput : MonoBehaviour
{
    public XRRig rig;
    public Bow bow;
    public float camSmoothX = 20;
    public float camSmoothY = 20;
    public float viewMax = 80;
    public float moveSpeed = 5;

    public int curArrowSelection = 0;

    Transform cameraT;
    XRControls xrControls;
    Vector2 inputCam = Vector2.zero;
    Vector2 inputMove = Vector2.zero;
    float inputSelect = 0;
    // bool inputFire = false;

    private void Awake()
    {
        rig.transform.SetParent(transform, false);
        rig.rig = gameObject;
        cameraT = Camera.main.transform.parent;

        // setup input
        xrControls = new XRControls();
        xrControls.Enable();
        xrControls.NonVR.AimCamera.performed += c => inputCam = c.ReadValue<Vector2>();
        xrControls.NonVR.AimCamera.canceled += c => inputCam = Vector2.zero;
        xrControls.NonVR.Move.performed += c => inputMove = c.ReadValue<Vector2>();
        xrControls.NonVR.Move.canceled += c => inputMove = Vector2.zero;
        xrControls.NonVR.ShootBow.performed += c => { NotchArrow(); };
        xrControls.NonVR.ShootBow.canceled += c => { Fire(); };
        xrControls.NonVR.ChooseArrow.performed += c => {
            inputSelect += Mathf.Sign(c.ReadValue<float>());
            inputSelect = Mathf.Clamp(inputSelect, 0, 20);
            curArrowSelection = Mathf.RoundToInt(inputSelect);
            // bow.arrowMenu.
        };

    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        Vector3 movement = new Vector3(inputMove.x, 0, inputMove.y);
        transform.Translate(movement * moveSpeed * Time.deltaTime);
        float camYaw = inputCam.x * camSmoothX * Time.deltaTime;
        float camPitch = -inputCam.y * camSmoothY * Time.deltaTime;
        transform.Rotate(0, camYaw, 0);
        cameraT.Rotate(camPitch, 0, 0);
        // clamp cam rot
        var sAng = cameraT.localEulerAngles.x;
        if (sAng > viewMax && sAng < 180)
        {
            cameraT.localEulerAngles = new Vector3(viewMax, 0, 0);
        } else
        if (sAng < 360 - viewMax && sAng > 180)
        {
            cameraT.localEulerAngles = new Vector3(360 - viewMax, 0, 0);
        }
    }
    void NotchArrow()
    {
        curArrowSelection = Mathf.Clamp(curArrowSelection, 0, bow.arrowMenu.arrowPrefabs.Length - 1);
        inputSelect = curArrowSelection;
        var arrow = bow.arrowMenu.GetArrow(curArrowSelection);
        var notch = bow.bowNotch;
        bow.interactionManager.ForceSelect(notch, arrow);
    }
    void Fire()
    {
        // Debug.Log("Fire");
        bow.bowNotch.UpdatePull(1f);// max force
        bow.bowNotch.ReleaseArrow();
    }
}