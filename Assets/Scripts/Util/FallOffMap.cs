using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FallOffMap : MonoBehaviour
{
    public bool justDestroy = false;
    float minHeight = -50f;
    public UnityEvent fallEvent;

    private void Update()
    {
        if (transform.position.y <= minHeight)
        {
            VRDebug.Log(name + " fell off the map!");
            if (justDestroy)
            {
                Destroy(gameObject);
            }
            fallEvent.Invoke();
        }
    }
}