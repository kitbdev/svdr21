using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;
using Shapes;

/// <summary>
/// General AI for all enemies
/// includes Movement, Attacking, head tracking, multiple states, and death logic
/// </summary>
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

    public MoveState IdleMoveState = MoveState.WANDER;
    public MoveState playerDetectedMoveState = MoveState.CHASING;

    public MoveMode moveMode = MoveMode.GROUND;
    public enum MoveMode
    {
        STATIONARY,
        GROUND,
        FLY,
    }
    /// <summary>
    /// used by movepattern to determine the current action
    /// </summary>
    [ReadOnly] public MoveState moveState = MoveState.IDLE;
    public enum MoveState
    {
        IDLE,
        CHASING,
        WANDER,
        CHARGING,
    }

    [Space]
    public float playerDetectionRadius = 20;
    public float playerForgetRadius = 21;

    // will move farther from target if below
    public float targetMinDist = 0;
    // will move closer to target if past
    public float targetMaxDist = 50;
    public float preferedHeight = 10;
    // just wander
    public float wanderMaxDistance = 3;
    public float wanderTimeoutDur = 3;
    public float wanderMaxDelay = 3;
    public float wanderCheckDist = 0.8f;
    float wanderTime = 0;
    float wanderTimeoutTime = 0;

    [ReadOnly] [SerializeField] bool isTooCloseToTarget = false;
    [ReadOnly] [SerializeField] bool isTooFarFromTarget = false;
    [ReadOnly] [SerializeField] bool playerDetected = false;
    [ReadOnly] [SerializeField] bool stopMoving = false;
    [ReadOnly] [SerializeField] bool isGrounded = false;
    [ReadOnly] [SerializeField] float curSpeed = 0;


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
    [Header("Other")]
    public Transform lookTarget;
    public float lookTargSmoothing = 10;
    Vector3 headHeight = Vector3.up * 1.5f;
    Transform playerHead;
    public bool alwaysLookAtPlayer = false;
    public GameObject deathGoDetach;
    public float deathGoDetachDestroyDelay = 50;
    public Transform headpos;
    public float loosePartReconnectDelay = 3;

    [Space]
    [ReadOnly] [SerializeField] bool playerDefeated = false;
    [ReadOnly] [SerializeField] bool isBeingKnockedBacked = false;

    [ReadOnly] public Transform moveTarget;
    [ReadOnly] public Transform attackTarget;
    protected Transform createdMoveTarget;
    [HideInInspector] public Health health;
    protected Transform playerT;
    protected Rigidbody rb;
    protected Animator anim;
    protected AttackAnimWatcher animWatcher;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        anim = GetComponentInChildren<Animator>();
        var sphereCol = gameObject.AddComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = playerDetectionRadius;

        playerT = GameManager.Instance.player;
        playerHead = Camera.main.transform;
        rb.useGravity = moveMode == MoveMode.GROUND;
        createdMoveTarget = new GameObject(name + "_created_move_target").transform;
        createdMoveTarget.position = transform.position;
        headHeight = headpos.position - transform.position;
        rb.centerOfMass = Vector3.zero;

        // setup attack stuff
        attackIndivCooldowns = new float[allAttacks.Length];
        if (!attackSpawnPoint)
        {
            attackSpawnPoint = transform;
        }
        animWatcher = GetComponentInChildren<AttackAnimWatcher>();
        attackTarget = playerT;

        if (deathGoDetach) ActivateRbs(deathGoDetach.transform, false);
    }
    private void OnEnable()
    {
        health.dieEvent.AddListener(Die);
        health.damageEvent.AddListener(OnHit);
    }
    void ResetAttackCooldowns()
    {
        attackCooldown = 0;
        for (int i = 0; i < attackIndivCooldowns.Length; i++)
        {
            attackIndivCooldowns[i] = 0;
        }
    }

    void Update()
    {
        curMoveBlockDur -= Time.deltaTime;

        // check if player is not detected
        if (playerDetected)
        {
            float playerDist = Vector3.Distance(playerT.position, transform.position);
            if (playerDist >= playerForgetRadius)
            {
                playerDetected = false;
                moveTarget = null;
            }
        }
        if (moveTarget == null)
        {
            moveTarget = createdMoveTarget;
        }
        UpdateLookAt();
        if (playerDefeated)
        {
            // no need to move or attack anymore
            return;
        }

        if (moveMode == MoveMode.GROUND)
        {
            CheckGrounded();
        }

        // set movestate based on player
        if (playerDetected)
        {
            moveState = playerDetectedMoveState;
        } else
        {
            moveState = IdleMoveState;
        }
        Move();

        // reduce attack cooldowns
        attackCooldown -= Time.deltaTime;
        for (int i = 0; i < attackIndivCooldowns.Length; i++)
        {
            attackIndivCooldowns[i] -= Time.deltaTime;
        }
        if (playerDetected)
        {
            // dont attack if player is not looking
            float forwardThreshold = -0.1f;
            if (Vector3.Dot(playerHead.forward, (transform.position - playerHead.position).normalized) < forwardThreshold)
            {
                return;
            }
            TryDoAttack();
        }
    }
    void UpdateLookAt()
    {
        // update looktarget
        Vector3 targLookPos = lookTarget.position;
        if (alwaysLookAtPlayer)
        {
            targLookPos = playerHead.position;
        } else
        {
            // targLookPos = transform.position + headHeight + Vector3.forward;
            if (playerDetected)
            {
                targLookPos = playerHead.position;
            } else
            {
                targLookPos = moveTarget.position;
                targLookPos += headHeight;
            }
        }
        if (lookTargSmoothing > 0)
        {
            lookTarget.position = Vector3.Lerp(lookTarget.position, targLookPos, lookTargSmoothing * Time.deltaTime);
        } else
        {
            lookTarget.position = targLookPos;
        }
        if (headpos && Vector3.Distance(headpos.position, lookTarget.position) < 0.5f)
        {
            // ? or reduce the weight
            var lookDir = headpos.position - lookTarget.position;
            lookDir.y = 0;
            lookTarget.position += -lookDir.normalized * 0.5f;
        }
    }
    public void PlayerDefeated()
    {
        playerDefeated = true;
        anim.SetBool("Laughing", true);
    }
    void Move()
    {
        // get target state
        float targetDist = Vector3.Distance(transform.position, moveTarget.position);
        // need to move farther
        isTooCloseToTarget = false;
        // need to move closer
        isTooFarFromTarget = false;
        if (targetDist > targetMaxDist)
        {
            isTooFarFromTarget = true;
        } else if (targetDist < targetMinDist)
        {
            isTooCloseToTarget = true;
        }

        // movestate logic
        switch (moveState)
        {
            case MoveState.WANDER:
                // wander around aimlessly
                if (targetDist <= wanderCheckDist || Time.time >= wanderTimeoutTime)
                {
                    // wait to get a new target
                    if (wanderTime <= 0)
                    {
                        // set new delay
                        float rDelay = Random.Range(1f, wanderMaxDelay);
                        wanderTime = Time.time + rDelay;
                        // lots of overshoots, but whatever
                        // Debug.Log("Wanderdelay " + rDelay);
                    } else if (Time.time >= wanderTime)
                    {
                        // get new target
                        moveTarget = createdMoveTarget;

                        Vector3 newTarg = transform.position;
                        switch (moveMode)
                        {
                            case MoveMode.STATIONARY:
                                break;
                            case MoveMode.GROUND:
                                Vector2 np2 = Random.insideUnitCircle * wanderMaxDistance;
                                newTarg = transform.position + new Vector3(np2.x, 0, np2.y);
                                break;
                            case MoveMode.FLY:
                                newTarg = transform.position + Random.insideUnitSphere * wanderMaxDistance;
                                if (Random.value > 0.5f)
                                {
                                    newTarg.y = preferedHeight;
                                }
                                break;
                        }
                        moveTarget.position = newTarg;
                        wanderTime = 0;
                        wanderTimeoutTime = Time.time + wanderTimeoutDur;
                    }
                    MoveTo(moveTarget.position, true);
                } else
                {
                    // move to current target
                    MoveTo(moveTarget.position);
                    wanderTime = 0;
                }
                break;

            case MoveState.CHASING:
                // follow the player
                Vector3 targPostion = transform.position;
                if (isTooCloseToTarget)
                {
                    Vector3 away = transform.position - moveTarget.position;
                    targPostion = away.normalized;
                } else if (isTooFarFromTarget)
                {
                    targPostion = moveTarget.position;
                } else
                {
                    MoveTo(moveTarget.position, true);
                }
                switch (moveMode)
                {
                    case MoveMode.STATIONARY:
                        break;
                    case MoveMode.GROUND:
                        break;
                    case MoveMode.FLY:
                        // targPostion.y = preferedHeight;
                        break;
                }
                MoveTo(targPostion);
                break;
            case MoveState.CHARGING:
                break;
            case MoveState.IDLE:
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                MoveTo(transform.position);
                break;
            default:
                break;
        }
    }
    void MoveTo(Vector3 targetPos, bool onlyRotate = false)
    {
        if (moveTarget == null)
        {
            return;
        }
        if (curMoveBlockDur > 0)
        {
            return;
        }
        // Debug.Log("mmg" + moveMode == MoveMode.GROUND + " g" + isGrounded);
        if (moveMode == MoveMode.GROUND && !isGrounded)
        {
            // cannot move in air
            // Debug.Log("In air!");
            return;
        }
        float distToTarg = Vector3.Distance(transform.position, targetPos);
        bool tooCloseToMove = distToTarg <= 0.01f;
        if (distToTarg <= 0.01f)
        {
            // too close, ignore
            return;
        }
        // Debug.Log("moving somewhere! " + targetPos);

        // rotate towards target
        float rotRate = turnSpeed * Time.deltaTime;
        Quaternion targRot = transform.rotation;
        Vector3 toTarg = targetPos - transform.position;
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
        }
        // Quaternion.Slerp
        targRot = Quaternion.RotateTowards(transform.rotation, targRot, rotRate);
        transform.rotation = targRot;

        if (onlyRotate)
        {
            return;
        }
        // if there is a pit in front of us, stop
        // ! ? not working at all
        // float forwardCheckDist = 0.1f;
        // Vector3 startGroundCheck = transform.position + Vector3.up * 0.05f + transform.forward * forwardCheckDist;
        // Debug.DrawRay(startGroundCheck, Vector3.down * 200f, Color.blue, 3);
        // if (Physics.Raycast(startGroundCheck, Vector3.down, out var hit, 0.1f, groundLayer, QueryTriggerInteraction.Ignore))
        // {
        // } else
        // {
        //     Debug.Log("didnt hit!");
        //     // did not hit anything in front of us
        //     return;
        // }
        // move
        switch (moveMode)
        {
            case MoveMode.STATIONARY:
                // only rotate, dont move
                return;
            case MoveMode.GROUND:
                break;
            case MoveMode.FLY:
                break;
        }
        if (isBeingKnockedBacked)
        {
            return;
        }
        var vel = rb.velocity;
        // will only move when facing target by at least 50%
        float facingAmount = Mathf.Clamp01(Vector3.Dot(transform.forward, toTarg.normalized) * 2 - 0.5f);
        float targSpeed = stopMoving ? 0 : moveSpeed * facingAmount;
        curSpeed = Mathf.Lerp(curSpeed, targSpeed, accelerationRate * Time.deltaTime);

        vel = transform.forward * curSpeed;
        vel = Vector3.ClampMagnitude(vel, moveSpeed);
        rb.velocity = vel;
    }
    private void OnTriggerStay(Collider other)
    {
        const string playerTag = GameManager.PlayerTag;
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
    protected void Knockback(Vector3 point, Vector3 vel)
    {
        float mag = vel.magnitude;
        // todo fix
        VRDebug.Log("Knockback " + mag);
        rb.AddExplosionForce(mag, point, 1, 1.5f);
    }
    void OnHit()
    {
        if (health.lastHitArgs.hit.TryGetComponent<Rigidbody>(out var hitrb))
        {
            if (loosePartReconnectDelay > 0)
            {
                // disconnect that rb
                hitrb.isKinematic = false;
                DOTween.To(() => hitrb.isKinematic.AsInt(), x => {
                    if (x >= 1)
                    {
                        hitrb.isKinematic = true;
                        hitrb.transform.DOLocalMove(Vector3.zero, 1);
                    }
                }, 1, loosePartReconnectDelay);
            }
        }
        Knockback(health.lastHitArgs.point, health.lastHitArgs.velocity);
    }
    protected void Die()
    {
        VRDebug.Log("Enemy " + name + " died");
        EnemyManager.Instance.EnemyDied(this);
        // anim
        if (deathGoDetach)
        {
            // activate rbs and deactivate anim
            anim.enabled = false;
            GetComponentInChildren<RigBuilder>().enabled = false;
            deathGoDetach.transform.SetParent(null);
            Destroy(deathGoDetach, deathGoDetachDestroyDelay);
            ActivateRbs(deathGoDetach.transform, true);
        }
        // todo change eye color
        Destroy(gameObject);
    }
    void ActivateRbs(Transform baseT, bool activate)
    {
        var rbs = baseT.GetComponentsInChildren<Rigidbody>();
        foreach (var baserb in rbs)
        {
            baserb.isKinematic = !activate;
        }
    }
    protected void CheckGrounded()
    {
        float maxDist = 0.1f;
        Vector3 startPos = transform.position + Vector3.up * maxDist / 2;
        Debug.DrawRay(startPos, Vector3.down * maxDist, Color.green);
        if (Physics.Raycast(startPos, Vector3.down, out var hit, maxDist, groundLayer))
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
        VRDebug.Log(name + "is attacking w/" + attack.name);
        // currentActiveAttacks.Add(attack);
        lastAttackTime = Time.time;
        if (attack.moveBlockDur > 0)
        {
            curMoveBlockDur = attack.moveBlockDur;
        }
        attack.Trigger(transform, attackSpawnPoint, anim);
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
        if (moveTarget) Draw.Sphere(moveTarget.position, 0.2f, Color.red);
    }
}
