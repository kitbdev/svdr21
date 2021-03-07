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
    public float turnSpeed = 10;
    public bool isOnGround = true;

    public float playerDetectionRadius = 20;
    public float playerForgetRadius = 25;
    public float stopRadius = 1;

    [ReadOnly] [SerializeField] bool playerDetected = false;
    [ReadOnly] [SerializeField] bool stop = false;
    protected Transform target;
    protected Rigidbody rb;
    protected Health health;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        var sphereCol = gameObject.AddComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = playerDetectionRadius;
    }
    private void OnEnable()
    {
        health.dieEvent.AddListener(Die);

    }

    void Update()
    {
        if (playerDetected)
        {
            float targetDist = Vector3.Distance(transform.position, target.position);
            if (targetDist >= playerForgetRadius)
            {
                playerDetected = false;
            } else if (targetDist <= stopRadius)
            {
                stop = true;
            }
        }
        Move();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.transform;
            playerDetected = true;
        }
    }
    void Move()
    {
        if (!playerDetected || stop)
        {
            return;
        }
        // todo fix
        float rotRate = turnSpeed * Time.deltaTime;
        Quaternion targRot;
        if (isOnGround)
        {
            // only rotate aroun y axis
            Vector3 toFlat = new Vector3(0, target.position.y - transform.position.y, 0);
            targRot = Quaternion.LookRotation(toFlat, Vector3.up);
        } else
        {
            // rotate freely
            targRot = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        }
        // Quaternion.Slerp
        targRot = Quaternion.RotateTowards(transform.rotation, targRot, rotRate);
        transform.rotation = targRot;
        rb.AddForce(transform.forward);
    }
    protected void Die()
    {

    }

    void OnDrawGizmosSelected()
    {
        Draw.Ring(transform.position, Vector3.up, playerDetectionRadius);
    }
}
