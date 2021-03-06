using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Bow : MonoBehaviour
{
    public Transform stringGrab;
    public LineRenderer bowstringLR;

    private void LateUpdate()
    {
        if (stringGrab.hasChanged)
        {
            UpdateLine();
        }
    }
    void UpdateLine()
    {
        Vector3 centerLoc = bowstringLR.transform.InverseTransformPoint(stringGrab.position);
        bowstringLR.SetPosition(1, centerLoc);

    }
}
