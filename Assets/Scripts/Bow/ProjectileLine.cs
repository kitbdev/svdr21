using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class ProjectileLine : MonoBehaviour, ILineRenderable
{

    const int k_MaxRaycastHits = 10;
    const int k_MinLineSamples = 2;
    const int k_MaxLineSamples = 100;

    public float projectileVelocity = 16f;
    public float gravityAcceleration = 9.8f;
    public float gravityMult = 1f;
    [SerializeField]
    float additionalFlightTime = 0.5f;
    [SerializeField]
    [Range(k_MinLineSamples, k_MaxLineSamples)]
    int m_SampleFrequency = 20;
    /// <summary>
    /// Gets or sets the number of sample points of the curve, should be at least 3,
    /// the higher the better quality.
    /// </summary>
    public int sampleFrequency
    {
        get => m_SampleFrequency;
        set {
            m_SampleFrequency = value;
            RebuildSamplePoints();
        }
    }
    /// <summary>
    /// Gets the signed angle between the forward direction and the tracking space.
    /// </summary>
    protected float angle
    {
        get {
            var castForward = startTransform.forward;
            var projectedForward = Vector3.ProjectOnPlane(castForward, m_ReferenceFrame.up);
            return Mathf.Approximately(Vector3.Angle(castForward, projectedForward), 0f)
                ? 0f
                : Vector3.SignedAngle(castForward, projectedForward, Vector3.Cross(castForward, projectedForward));
        }
    }

    /// <summary>
    /// Sets which shape of physics cast to use for the cast when detecting collisions.
    /// </summary>
    public enum HitDetectionType
    {
        /// <summary>
        /// Uses <see cref="Physics.Raycast"/> to detect collisions.
        /// </summary>
        Raycast,

        /// <summary>
        /// Uses <see cref="Physics.Spherecast"/> to detect collisions.
        /// </summary>
        SphereCast,
    }
    [SerializeField]
    HitDetectionType m_HitDetectionType = HitDetectionType.Raycast;
    /// <summary>
    /// Sets which type of hit detection to use for the raycast.
    /// </summary>
    public HitDetectionType hitDetectionType
    {
        get => m_HitDetectionType;
        set => m_HitDetectionType = value;
    }
    [SerializeField]
    [Range(0.01f, 0.25f)]
    float m_SphereCastRadius = 0.1f;
    /// <summary>
    /// Gets or sets radius used for sphere casting. Will use regular raycasting if set to 0 or less.
    /// </summary>
    public float sphereCastRadius
    {
        get => m_SphereCastRadius;
        set => m_SphereCastRadius = value;
    }

    [SerializeField]
    LayerMask m_RaycastMask = -1;
    /// <summary>
    /// Gets or sets layer mask used for limiting raycast targets.
    /// </summary>
    public LayerMask raycastMask
    {
        get => m_RaycastMask;
        set => m_RaycastMask = value;
    }
    [SerializeField]
    QueryTriggerInteraction m_RaycastTriggerInteraction = QueryTriggerInteraction.Ignore;
    /// <summary>
    /// Gets or sets type of interaction with trigger volumes via raycast.
    /// </summary>
    public QueryTriggerInteraction raycastTriggerInteraction
    {
        get => m_RaycastTriggerInteraction;
        set => m_RaycastTriggerInteraction = value;
    }

    [SerializeField]
    Transform m_ReferenceFrame;
    public bool lineActive = true;

    // reusable array of raycast hits
    readonly RaycastHit[] m_RaycastHits = new RaycastHit[k_MaxRaycastHits];
    // reusable list of sample points
    Vector3[] m_SamplePoints;
    int m_NoSamplePoints = -1;

    // state to manage hover selection
    XRBaseInteractable m_CurrentNearestObject;
    float m_LastTimeHoveredObjectChanged;
    bool m_PassedHoverTimeToSelect;

    int m_HitCount;
    int m_HitPositionInLine = -1;
    Transform startTransform;

    private void Awake()
    {
        startTransform = transform;
    }
    protected void OnEnable()
    {
        RebuildSamplePoints();
        FindReferenceFrame();
    }

    protected void OnDisable()
    {
        // Clear lines
        m_NoSamplePoints = -1;
    }

    void RebuildSamplePoints()
    {
        int samplePointsSize = m_SampleFrequency;
        if (m_SamplePoints == null || m_SamplePoints.Length != samplePointsSize)
            m_SamplePoints = new Vector3[samplePointsSize];
        m_NoSamplePoints = 0;
    }

    void FindReferenceFrame()
    {
        if (m_ReferenceFrame != null)
        {
            return;
        }
        m_ReferenceFrame = new GameObject("Projectile World Reference").transform;

        // m_ReferenceFrame = transform;
    }
    int CheckCollidersBetweenPoints(Vector3 from, Vector3 to)
    {
        Array.Clear(m_RaycastHits, 0, k_MaxRaycastHits);

        // Cast from last point to next point to check if there are hits in between
        if (m_HitDetectionType == HitDetectionType.SphereCast && m_SphereCastRadius > 0f)
        {
            return Physics.SphereCastNonAlloc(from, m_SphereCastRadius, (to - from).normalized,
                m_RaycastHits, Vector3.Distance(to, from), raycastMask, raycastTriggerInteraction);
        }

        return Physics.RaycastNonAlloc(from, (to - from).normalized,
            m_RaycastHits, Vector3.Distance(to, from), raycastMask, raycastTriggerInteraction);
    }
    static Vector3 CalculateProjectilePoint(float t, Vector3 start, Vector3 velocity, Vector3 acceleration)
    {
        return start + velocity * t + 0.5f * acceleration * t * t;
    }
    private void Update()
    {
        if (lineActive)
        {
            SampleLine();
        }
    }

    void SampleLine()
    {
        // If we haven't initialized cleanly, bail out
        if (m_SamplePoints == null || m_SamplePoints.Length < 2)
        {
            return;
        }

        m_NoSamplePoints = 1;
        m_SamplePoints[0] = startTransform.position;

        // Pointers used to sample the curve and check colliders between points
        Vector3 previousPoint = m_SamplePoints[0];
        Vector3 nextPoint;

        m_HitCount = 0;
        m_HitPositionInLine = 0;
        int accumulatedHits = 0;
        int maxSamplePoints;

        // projectile logic
        float acceleration = gravityAcceleration * gravityMult;
        float flightTime = 2f * projectileVelocity * Mathf.Sin(Mathf.Abs(angle) * Mathf.Deg2Rad) / acceleration + additionalFlightTime;
        Vector3 velocityVector = startTransform.forward * projectileVelocity;
        Vector3 accelerationVector = m_ReferenceFrame.up * -1f * acceleration;

        maxSamplePoints = m_SamplePoints.Length;
        accumulatedHits = 0;
        for (int i = 1; i < m_SampleFrequency && m_NoSamplePoints < maxSamplePoints; ++i)
        {
            float t = i / (float)(m_SampleFrequency - 1) * flightTime;

            nextPoint = CalculateProjectilePoint(t, startTransform.position, velocityVector, accelerationVector);

            // Check collider only when there has not been a hit point
            if (accumulatedHits == 0)
            {
                accumulatedHits += CheckCollidersBetweenPoints(previousPoint, nextPoint);
                if (accumulatedHits != 0)
                    m_HitPositionInLine = i;
            }

            // Keep sampling
            m_SamplePoints[m_NoSamplePoints] = nextPoint;
            m_NoSamplePoints++;
            previousPoint = nextPoint;
        }
        m_HitCount = accumulatedHits;


        // Save sample points as the local points of the startTransform,
        // when accessing the sample points at a different time they will have to be transformed into world space.
        for (int i = 0; i < m_SamplePoints.Length; ++i)
        {
            m_SamplePoints[i] = startTransform.InverseTransformPoint(m_SamplePoints[i]);
        }

    }

    /// <summary>
    /// ILineRenderer interface
    /// </summary>
    /// <param name="linePoints"></param>
    /// <param name="numPoints"></param>
    /// <returns></returns>
    public bool GetLinePoints(ref Vector3[] linePoints, out int numPoints)
    {
        if (!lineActive || m_SamplePoints == null || m_SamplePoints.Length < 2 || m_NoSamplePoints < 2)
        {
            numPoints = default;
            return false;
        }

        if (linePoints == null || linePoints.Length != m_NoSamplePoints)
        {
            linePoints = new Vector3[m_NoSamplePoints];
        }

        // Transform samples points from local to world space
        for (int i = 0; i < m_SamplePoints.Length; ++i)
        {
            linePoints[i] = startTransform.TransformPoint(m_SamplePoints[i]);
        }

        numPoints = m_NoSamplePoints;
        // Note: if array exception is generated, its something to do with domain reloading
        // reentering play mode should fix it
        return true;
    }
    /// <summary>
    /// This function will return the first raycast result, if any raycast results are available.
    /// </summary>
    /// <param name="raycastHit">When this method returns, contains the raycast result if available; otherwise, the default value.</param>
    /// <returns>Returns <see langword="true"/> if the <paramref name="raycastHit"/> parameter contains a valid raycast result.
    /// Otherwise, returns <see langword="false"/>.</returns>
    public bool GetCurrentRaycastHit(out RaycastHit raycastHit)
    {
        if (m_HitCount > 0 && m_RaycastHits.Length > 0)
        {
            raycastHit = m_RaycastHits[0];
            return true;
        }

        raycastHit = default;
        return false;
    }
    /// <summary>
    /// ILineRenderer interface
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normal"></param>
    /// <param name="positionInLine"></param>
    /// <param name="isValidTarget"></param>
    /// <returns></returns>
    public bool TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget)
    {
        position = default;
        normal = default;
        positionInLine = default;
        isValidTarget = false;
        var isValidRaycast = false;

        var distance = float.MaxValue;
        var rayIndex = int.MaxValue;

        if (GetCurrentRaycastHit(out var raycastHit))
        {
            position = raycastHit.point;
            normal = raycastHit.normal;
            rayIndex = m_HitPositionInLine;
            positionInLine = m_HitPositionInLine;
            distance = raycastHit.distance;

            isValidRaycast = true;

            isValidTarget = true;
        }

        return isValidRaycast;
    }
}