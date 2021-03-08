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
    [Header("Movement")]
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
    /// <summary>
    /// type of ai. determines how the ai moves between states
    /// </summary>
    public MovePattern movePattern = MovePattern.CHASE;
    public enum MovePattern
    {
        NONE,
        CHASE,
        CIRCLE,
        WANDER,
    }
    /// <summary>
    /// used by movepattern to determine the current action
    /// </summary>
    [ReadOnly] public MoveState moveState = MoveState.IDLE;
    public enum MoveState
    {
        IDLE,
        CHARGING,
        CHASING,
        ATTACKING,
        WANDER,
    }


    [Space]
    public float playerDetectionRadius = 20;
    public float playerForgetRadius = 21;
    public float stopRadius = 2;
    // for rangers
    public float preferredRadius = 10;
    public float stopResumeRadius = 2.5f;
    public float preferedHeight = 10;
    public float wanderMaxDistance = 3;
    // public float stopResumeDelay = 0.5f;
    // float stopResumeTime = 0;

    [Header("Combat")]
    public float damage = 1;
    public float attackRate = 1;
    float lastAttackTime = 1;
    public Transform attackSpawnPoint;
    float curMoveBlockDur = 0;
    // todo attack types
    /*
     attacking
     attack and moveing are seperate
     if in has melee attacks and in melee range use that
     else if the has range attacks, use them 
     this affects animation and may block movement while occuring
    */
    public AttackSO[] allAttacks = new AttackSO[0];
    [ReadOnly] [SerializeField] protected List<AttackSO> currentActiveAttacks = new List<AttackSO>();
    [ReadOnly] [SerializeField] float attackCooldown = 0;
    [ReadOnly] [SerializeField] float[] attackIndivCooldowns;

    [Space]
    [ReadOnly] [SerializeField] bool playerDetected = false;
    [ReadOnly] [SerializeField] bool stopMoving = false;
    [ReadOnly] [SerializeField] bool isGrounded = false;
    [ReadOnly] [SerializeField] float curSpeed = 0;
    [ReadOnly] [SerializeField] bool isBeingKnockedBacked = false;

    [ReadOnly] public Transform moveTarget;
    protected Transform createdMoveTarget;
    [ReadOnly] public Transform attackTarget;
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

        // setup attack stuff
        attackIndivCooldowns = new float[allAttacks.Length];
    }
    private void OnEnable()
    {
        health.dieEvent.AddListener(Die);
        health.damageEvent.AddListener(OnHit);
    }

    void Update()
    {
        curMoveBlockDur -= Time.deltaTime;
        if (moveTarget != null)
        {
            float targetDist = Vector3.Distance(transform.position, moveTarget.position);
            if (playerDetected)
            {
                if (targetDist >= playerForgetRadius)
                {
                    playerDetected = false;
                    moveTarget = null;
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
            // todo perfered radius
            if (moveMode == MoveMode.GROUND)
            {
                CheckGrounded();
            }
            Move();
        }
        TryDoAttack();
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
            moveTarget = other.transform;
            playerDetected = true;
        }
    }
    void Move()
    {
        if (moveTarget == null)
        {
            return;
        }
        if (curMoveBlockDur > 0)
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
        Vector3 toTarg = moveTarget.position - transform.position;
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
            // todo dont fall into pits

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
    protected void TryDoAttack()
    {
        // choose an attack
        if (!attackTarget)
        {
            return;
        }
        if (attackCooldown > 0)
        {
            return;
        }
        float dist = Vector3.Distance(attackTarget.position, transform.position);
        // expect attacks to be sorted by priority
        for (int i = 0; i < allAttacks.Length; i++)
        {
            if (attackIndivCooldowns[i] > 0)
            {
                continue;
            }
            AttackSO attack = allAttacks[i];
            // todo randomization so not the same one is chosen each time?
            // dist check
            if (attack.minDist >= 0)
            {
                if (dist < attack.minDist)
                {
                    continue;
                }
            }
            if (attack.maxDist >= 0)
            {
                if (dist > attack.maxDist)
                {
                    continue;
                }
            }
            PerformAttack(attack, i);
            break;
        }
    }

    protected void PerformAttack(AttackSO attack, int index)
    {
        VRDebug.Log("Enemy " + name + " attack w/" + attack.name);
        // currentActiveAttacks.Add(attack);
        lastAttackTime = Time.time;
        if (attack.moveBlockDur > 0)
        {
            curMoveBlockDur = attack.moveBlockDur;
        }
        if (attack.spawnPrefab)
        {
            var spawnGo = Instantiate(attack.spawnPrefab);
            spawnGo.transform.position = attackSpawnPoint.position;
            spawnGo.transform.rotation = attackSpawnPoint.rotation;
            if (attack.keepAttached)
            {
                spawnGo.transform.SetParent(transform);
            }
            // todo launch dist
        }
        if (attack.launchEffectPrefab)
        {
            var spawnGo = Instantiate(attack.launchEffectPrefab);
            spawnGo.transform.position = attackSpawnPoint.position;
            spawnGo.transform.rotation = attackSpawnPoint.rotation;
        }
        if (attack.cooldown >= 0)
        {
            attackCooldown = attack.cooldown;
        }
        if (attack.individualCoolDown >= 0)
        {
            attackIndivCooldowns[index] = attack.individualCoolDown;
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Draw.Ring(transform.position, Vector3.up, playerDetectionRadius);
    }
}
