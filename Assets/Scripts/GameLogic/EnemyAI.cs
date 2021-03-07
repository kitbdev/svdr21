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
    public LayerMask groundLayer = Physics.DefaultRaycastLayers;

    public MoveMode moveMode = MoveMode.GROUND;
    public enum MoveMode
    {
        STATIONARY,
        GROUND,
        FLY,
    }
    public enum MovePattern {
        NONE,
        FOLLOW_TARGET,
        CIRCLE,
        WANDER,
    }
    public enum MoveState {
        IDLE,
        CHARGING,
        CHASING,
        ATTACKING,
    }

    [Space]
    public float playerDetectionRadius = 20;
    public float playerForgetRadius = 21;
    public float stopRadius = 2;
    public float stopResumeRadius = 2.5f;
    public float preferedHeight = 10;
    // public float stopResumeDelay = 0.5f;
    // float stopResumeTime = 0;

    [ReadOnly] [SerializeField] bool playerDetected = false;
    [ReadOnly] [SerializeField] bool stopMoving = false;
    [ReadOnly] [SerializeField] bool isGrounded = false;
    [ReadOnly] [SerializeField] float curSpeed = 0;
    [ReadOnly] [SerializeField] bool isBeingKnockedBacked = false;

    [ReadOnly] public Transform target;
    [HideInInspector] public Health health;
    protected Transform playerT;
    protected Rigidbody rb;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        var sphereCol = gameObject.AddComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = playerDetectionRadius;

        playerT = GameManager.Instance.player;
        rb.useGravity = moveMode == MoveMode.GROUND;
    }
    private void OnEnable()
    {
        health.dieEvent.AddListener(Die);
        health.damageEvent.AddListener(OnHit);
    }

    void Update()
    {
        if (target != null)
        {
            float targetDist = Vector3.Distance(transform.position, target.position);
            if (playerDetected)
            {
                if (targetDist >= playerForgetRadius)
                {
                    playerDetected = false;
                    target = null;
                    return;
                }
            }
            if (targetDist <= stopRadius)
            {
                if (!stopMoving)
                {
                    stopMoving = true;
                }
            } else if (targetDist > stopResumeRadius)
            {
                if (stopMoving)
                {
                    stopMoving = false;
                }
            }
            if (moveMode == MoveMode.GROUND)
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
        if (target == null)
        {
            return;
        }
        // todo idle move?
        if (moveMode == MoveMode.GROUND && !isGrounded)
        {
            // cannot move in air
            return;
        }

        // rotate
        float rotRate = turnSpeed * Time.deltaTime;
        Quaternion targRot = transform.rotation;
        Vector3 toTarg = target.position - transform.position;
        if (moveMode == MoveMode.GROUND)
        {
            // only rotate aroun y axis
            Vector3 toFlat = toTarg;
            toFlat.y = 0;
            targRot = Quaternion.LookRotation(toFlat, Vector3.up);
        } else if (moveMode == MoveMode.FLY)
        {
            // rotate freely
            targRot = Quaternion.LookRotation(toTarg.normalized, Vector3.up);
            // todo target height
        }
        // Quaternion.Slerp
        targRot = Quaternion.RotateTowards(transform.rotation, targRot, rotRate);
        transform.rotation = targRot;

        // move
        if (moveMode == MoveMode.STATIONARY)
        {
            return;
        } else if (moveMode == MoveMode.GROUND)
        {

        } else if (moveMode == MoveMode.STATIONARY)
        {

        }
        if (isBeingKnockedBacked)
        {
            return;
        }
        var vel = rb.velocity;
        // wait until facing player
        float facingAmount = Mathf.Clamp01(Vector3.Dot(transform.forward, toTarg.normalized) + 0.3f);
        float targSpeed = stopMoving ? 0 : moveSpeed * facingAmount;
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
