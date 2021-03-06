using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class HandPresence : MonoBehaviour
{

    public bool isLeft = false;
    [Header("Input")]
    public InputAction grip = new InputAction("Grip", InputActionType.Value, binding: "<XRController>{RightHand}/grip");
    public InputAction trigger = new InputAction("Trigger", InputActionType.Value, binding: "<XRController>{RightHand}/trigger");
    public InputAction gripTouched = new InputAction("GripTouch", InputActionType.Button, binding: "<XRController>{RightHand}/gripTouched");
    public InputAction triggerTouched = new InputAction("TriggerTouch", InputActionType.Button, binding: "<XRController>{RightHand}/triggerTouched");
    // public InputAction primaryTouched = new InputAction("PrimaryTouch", InputActionType.Button, binding: "<XRController>{RightHand}/primaryTouched");
    // [Space]
    [Tooltip("point root for UI check")]
    [Header("UI check")]
    public Transform pointer;
    public bool checkForUI = true;
    public bool pointAtUI = true;
    public float maxUIDist = 0.5f;
    public float uiThroughDist = 0.1f;
    public LayerMask uiLayer = 1 << 5;
    public GameObject UIRayInteractor;
    [Header("Model Settings")]
    [Tooltip("model base that will be reparented")]
    public GameObject model;
    public bool disableModelOnGrab = false;
    public float animTriggerSmoothing = 100f;
    [Header("Other")]
    public bool handPhysics = false;

    float lastAnimTriggerVal = 0;
    float gripVal;
    float triggerVal;
    // bool gripTouching;
    bool triggerTouching;
    int grabVal;
    bool uiInRange = false;
    Transform modelOrigParent;
    bool modelTaken = false;
    bool isPointingOverride = false;
    Animator animator;
    Rigidbody rb;
    ConfigurableJoint configurableJoint;

    private void Awake()
    {
        animator = model.GetComponentInChildren<Animator>();

        modelOrigParent = model.transform.parent;
        if (handPhysics)
        {
            rb = GetComponentInChildren<Rigidbody>();
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // rb.freezeRotation = true;
            configurableJoint = rb.GetComponent<ConfigurableJoint>();
            //todo
            rb.transform.SetParent(transform.parent.parent.parent);
        } else
        {
            // var r = GetComponentInChildren<Rigidbody>();
            // Destroy(r.gameObject.GetComponent<ConfigurableJoint>());
            // Destroy(r);
        }
        if (pointer == null)
        {
            pointer = transform;
        }

        // setup input
        gripTouched.Enable();
        triggerTouched.Enable();
        // gripTouched.performed += c => { gripTouching = true; UpdateAnimGrip(); };
        // gripTouched.canceled += c => { gripTouching = false; UpdateAnimGrip(); };
        triggerTouched.performed += c => { triggerTouching = true; UpdateAnimTrgger(); };
        triggerTouched.canceled += c => { triggerTouching = false; UpdateAnimTrgger(); };
        grip.Enable();
        trigger.Enable();
        grip.performed += c => { gripVal = c.ReadValue<float>(); UpdateAnimGrip(); };
        grip.canceled += c => { gripVal = 0; UpdateAnimGrip(); };
        trigger.performed += c => { triggerVal = c.ReadValue<float>(); UpdateAnimTrgger(); };
        trigger.canceled += c => { triggerVal = 0; UpdateAnimTrgger(); };
    }
    [ContextMenu("SwitchToLeft")]
    void SwitchToLeftInput()
    {
        SwitchInputPaths("Right", "Left");
        isLeft = true;
    }
    [ContextMenu("SwitchToRight")]
    void SwitchToRightInput()
    {
        SwitchInputPaths("Left", "Right");
        isLeft = false;
    }
    void SwitchInputPaths(string fromStr, string toStr)
    {
        InputAction[] allInputActions = new InputAction[]{
            grip,
            trigger,
            gripTouched,
            triggerTouched,
        };
        foreach (var inputAction in allInputActions)
        {
            for (int i = 0; i < inputAction.bindings.Count; i++)
            {
                var b = inputAction.bindings[i];
                b.path = b.path.Replace(fromStr, toStr);
                inputAction.ChangeBinding(i).To(b);
            }
        }
    }
    private void Start()
    {
        if (UIRayInteractor) UIRayInteractor.SetActive(false);
    }
    void UpdateAnimGrip()
    {
        // float halfPoint = 0.5f;
        float gval = gripVal;
        // if (gripTouching) {
        // gval = gripVal / 2 + halfPoint;
        // }
        if (animator) animator.SetFloat("Grip", gval);
    }
    void UpdateAnimTrgger()
    {
        float halfPoint = 0.5f;
        float tval = 0;
        tval = triggerVal / 2 + halfPoint;
        if (!triggerTouching && triggerVal <= 0.01f)
        {
            tval = 0;
        }
        // smoothing
        if (animTriggerSmoothing > 0)
        {
            tval = Mathf.Lerp(lastAnimTriggerVal, tval, Time.unscaledDeltaTime * animTriggerSmoothing);
        }
        if (animator) animator.SetFloat("Trigger", tval);
        lastAnimTriggerVal = tval;
    }

    public void SetAnimGrab(bool grabbed)
    {
        SetAnimGrab(grabbed ? 1 : 0);
    }
    public void SetAnimGrab(int grabbed = 0)
    {
        grabVal = grabbed;
        if (disableModelOnGrab)
        {
            model.SetActive(grabbed != 0);
        } else
        {
            // different grabbed values are for different grab types
            if (animator) animator.SetInteger("Grab", grabbed);
        }
    }

    private void Update()
    {
        CheckUIRange();
        // animator.SetFloat("Grip", gripVal);
        // animator.SetFloat("Trigger", triggerVal);
        // CheckHandDist();
    }
    void CheckUIRange()
    {
        if (isPointingOverride)
        {
            return;
        }
        // check if near ui
        // Debug.DrawRay(pointer.position, pointer.forward * maxUIDist, Color.red);
        // note: needs UI to have a collider in UI layer
        bool canInteractWithUI = checkForUI && grabVal == 0 && !modelTaken;
        if (canInteractWithUI && Physics.Raycast(pointer.position - pointer.forward * uiThroughDist, pointer.forward, out RaycastHit hit, maxUIDist + uiThroughDist, uiLayer.value))
        {
            if (uiInRange)
            {
                // todo physically interact with UI??
            } else
            {
                uiInRange = true;
                if (pointAtUI) animator.SetBool("Point", true);
                if (UIRayInteractor) UIRayInteractor.SetActive(true);
            }
        } else if (uiInRange)
        {
            uiInRange = false;
            if (pointAtUI) animator.SetBool("Point", false);
            if (UIRayInteractor) UIRayInteractor.SetActive(false);
        }
    }
    public void SetPointing(bool pointing)
    {
        if (isPointingOverride != pointing)
        {
            isPointingOverride = pointing;
            animator.SetBool("Point", isPointingOverride);
        }
    }
}
