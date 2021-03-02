using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    [SerializeField] float moveSpeed = 10;
    [SerializeField] float turnSpeed = 10;
    [SerializeField] LayerMask groundLayer = ~(1 << 2);
    [SerializeField] MoveMode curMoveMode = MoveMode.NONE;

    enum MoveMode
    {
        NONE,
        HOVER,
        RAIL,
        ANTIGRAV,
        FLIGHT,
        DEAD,
    }

    [ReadOnly] [SerializeField] Vector3 centerPos = Vector3.zero;
    [SerializeField] float maxDist = 0.5f;
    [SerializeField] float deadzone = 0.1f;
    [SerializeField] float duckThreshold = 0.5f;


    // input
    [Header("Input")]
    [ReadOnly] [SerializeField] Vector3 headPos;
    [ReadOnly] [SerializeField] Vector3 lhandPos;
    [ReadOnly] [SerializeField] Vector3 rhandPos;
    [ReadOnly] [SerializeField] bool isGrounded = false;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {

    }

    public void Recenter()
    {
        centerPos = headPos;
    }
    void Update()
    {
        CheckGrounded();
        MoveDef();
    }
    void MoveDef()
    {

    }
    void CheckGrounded()
    {
        if (Physics.SphereCast(transform.position, 0.1f, -transform.up, out var hit, 0.2f, groundLayer.value))
        {

        }
    }
    private void OnDrawGizmosSelected() {
            
    }
}
