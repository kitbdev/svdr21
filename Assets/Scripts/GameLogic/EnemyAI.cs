using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Shapes;

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 10;
    public float accelerationRate = 10;
    // public float deaccelerationRate = 10;
    public float turnSpeed = 100;
    public bool isGroundBased = true;
    public LayerMask groundLayer = Physics.DefaultRaycastLayers;
    float curSpeed = 0;

    public float playerDetectionRadius = 20;
    public float playerForgetRadius = 21;
    public float stopRadius = 2;
    public float stopResumeRadius = 2.5f;
    // public float stopResumeDelay = 0.5f;
    // float stopResumeTime = 0;

    [ReadOnly] [SerializeField] bool playerDetected = false;
    [ReadOnly] [SerializeField] bool stop = false;
    [ReadOnly] [SerializeField] bool isGrounded = false;
    public Transform target;
    protected Transform playerT;
    protected Rigidbody rb;
    protected Health health;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        var sphereCol = gameObject.AddComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = playerDetectionRadius;

        playerT = GameManager.Instance.player;
        rb.useGravity = isGroundBased;
    }
    private void OnEnable()
    {
        health.dieEvent.AddListener(Die);
        health.damageEvent.AddListener(OnHit);
    }

    void Update()
    {
        if (playerDetected)
        {
            float targetDist = Vector3.Distance(transform.position, target.position);
            if (targetDist >= playerForgetRadius)
            {
                playerDetected = false;
            }
            if (targetDist <= stopRadius)
            {
                if (!stop)
                {
                    stop = true;
                }
            } else if (targetDist > stopResumeRadius)
            {
                if (stop)
                {
                    stop = false;
                }
            }
            // if have a target
            if (isGroundBased)
            {
                CheckGrounded();
            }
            Move();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        const string playerTag = "Player";
        if (!playerDetected && other.CompareTag(playerTag))
        {
            // check los
            Vector3 toDir = other.transform.position - transform.position;
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, toDir, out var hit, playerDetectionRadius, groundLayer))
            {
                // collided with something
                if (!hit.collider.CompareTag(playerTag))
                {
                    // hit something else
                    return;
                }
            }
            target = other.transform;
            playerDetected = true;
        }
    }
    void Move()
    {
        if (!playerDetected)
        {
            // todo idle move?
            return;
        }
        if (isGroundBased && !isGrounded)
        {
            // cannot move in air
            return;
        }

        // rotate
        float rotRate = turnSpeed * Time.deltaTime;
        Quaternion targRot;
        Vector3 toTarg = target.position - transform.position;
        if (isGroundBased)
        {
            // only rotate aroun y axis
            Vector3 toFlat = toTarg;
            toFlat.y = 0;
            targRot = Quaternion.LookRotation(toFlat, Vector3.up);
        } else
        {
            // rotate freely
            targRot = Quaternion.LookRotation(toTarg.normalized, Vector3.up);
            // todo should be always moving
        }
        // Quaternion.Slerp
        targRot = Quaternion.RotateTowards(transform.rotation, targRot, rotRate);
        transform.rotation = targRot;

        // move
        // todo wait until facing player
        // rb.AddForce(transform.forward * moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
        var vel = rb.velocity;
        float facingAmount = Mathf.Clamp01(Vector3.Dot(transform.forward, toTarg.normalized) + 0.3f);
        float targSpeed = stop ? 0 : moveSpeed * facingAmount;
        curSpeed = Mathf.Lerp(curSpeed, targSpeed, accelerationRate * Time.deltaTime);

        vel = transform.forward * curSpeed;
        vel = Vector3.ClampMagnitude(vel, moveSpeed);
        rb.velocity = vel;
    }
    protected void Knockback(Vector3 point, Vector3 vel)
    {
        float mag = vel.magnitude;
        VRDebug.Log("Knockback " + mag);
        rb.AddExplosionForce(mag, point, 1, 1.5f);
    }
    void OnHit()
    {
        Knockback(health.lastHitArgs.point, health.lastHitArgs.velocity);
    }
    protected void Die()
    {
        VRDebug.Log("Enemy " + name + " died");
        // todo anim
        Destroy(gameObject);
    }
    protected void CheckGrounded()
    {
        float maxDist = 0.1f;
        if (Physics.Raycast(transform.position + Vector3.up * maxDist / 2, Vector3.down, out var hit, maxDist, groundLayer))
        {
            isGrounded = true;
        } else
        {
            isGrounded = false;
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Draw.Ring(transform.position, Vector3.up, playerDetectionRadius);
    }
}
