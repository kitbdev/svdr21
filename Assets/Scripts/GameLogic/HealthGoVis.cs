using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Visualize Health using GameObjects
/// disables a gameobject from the list for every health lost
/// </summary>
public class HealthGoVis : MonoBehaviour
{
    public List<GameObject> visGos = new List<GameObject>();
    public int visOffset = 0;
    public bool deactivateOnLow = true;
    public bool disableKinematicOnLow = false;
    [HideInInspector] [SerializeField] List<Rigidbody> visRbs = new List<Rigidbody>();
    Health health;

    private void OnEnable()
    {
        health = GetComponent<Health>();
        health.healthUpdateEvent.AddListener(HealthUpdate);
        // todo also destroy it in a second?
        // get rbs
        if (disableKinematicOnLow)
        {
            for (int i = 0; i < visGos.Count; i++)
            {
                if (visGos[i].TryGetComponent<Rigidbody>(out var rb))
                {
                    visRbs.Add(rb);
                } else
                {
                    visRbs.Add(null);
                }
            }
        }
    }
    void HealthUpdate()
    {
        for (int i = 0; i < visGos.Count; i++)
        {
            bool isAbove = health.currentHealth > i + visOffset;
            if (deactivateOnLow)
            {
                visGos[i].SetActive(isAbove);
            }
            if (disableKinematicOnLow)
            {
                if (visRbs[i]) visRbs[i].isKinematic = isAbove;
            }
        }
    }
}