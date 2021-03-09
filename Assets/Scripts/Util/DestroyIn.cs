using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIn : MonoBehaviour
{
    public float destroyDelayDur = -1;
    private void Start()
    {
        DestroyAfter(destroyDelayDur);
    }
    public void DestroyAfter(float dur)
    {
        if (destroyDelayDur >= 0)
        {
            Destroy(gameObject, dur);
        }
    }
}