using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Generic trigger for many use cases
/// </summary>
[AddComponentMenu("_Util/Trigger")]
public class Trigger : MonoBehaviour
{

    [ContextMenuItem("Set to player tag", "SetTagToPlayer")]
    [ContextMenuItem("Clear tag", "ClearTag")]
    public string checkTag = "";
    public float repeatDur = -1;
    protected float lastTriggerRepTime = 0;
    protected float lastTriggerEnterTime = 0;
    public bool onlyOnce = false;
    // public LayerMask validLayers = Physics.DefaultRaycastLayers;

    [ReadOnly] public int numInTrigger = 0;
    [ReadOnly] public GameObject latestEnterGO = null;
    /// <summary>true when any are in the trigger. from first in to last out</summary>
    public bool inTrigger => numInTrigger > 0;
    public float durInTrigger => inTrigger ? Time.time - lastTriggerEnterTime : -1;

    [Header("Events")]
    public UnityEvent triggerEnteredEvent;
    public UnityEvent triggerStayEvent;
    public UnityEvent triggerExitEvent;

    [ContextMenu("Clear tag")]
    void ClearTag()
    {
        checkTag = "";
    }
    [ContextMenu("Set to player tag")]
    void SetTagToPlayer()
    {
        checkTag = GameManager.PlayerTag;
    }
    private void Awake()
    {
        numInTrigger = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTrig(other))
        {
            Debug.Log("Trigger enter " + name + " o:" + other);
            triggerEnteredEvent.Invoke();
            numInTrigger++;
            lastTriggerEnterTime = Time.time;
            latestEnterGO = other.gameObject;
            if (onlyOnce)
            {
                gameObject.SetActive(false);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (IsValidTrig(other))
        {
            if (repeatDur > 0)
            {
                if (Time.time <= lastTriggerRepTime + repeatDur)
                {
                    return;
                }
            }
            lastTriggerRepTime = Time.time;
            triggerStayEvent.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (IsValidTrig(other))
        {
            triggerExitEvent.Invoke();
            numInTrigger--;
        }
    }
    protected bool IsValidTrig(Collider other)
    {
        bool isValid = true;
        if (checkTag.Length > 0)
        {
            if (!other.CompareTag(checkTag))
            {
                isValid = false;
            }
        }
        return isValid;
    }
}