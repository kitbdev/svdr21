using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthGoVis : MonoBehaviour
{
    public List<GameObject> visGos = new List<GameObject>();
    public int visOffset = 0;
    Health health;
    private void OnEnable()
    {
        health = GetComponent<Health>();
        health.healthUpdateEvent.AddListener(HealthUpdate);
    }
    void HealthUpdate()
    {
        for (int i = 0; i < visGos.Count; i++)
        {
            visGos[i].SetActive(health.currentHealth > i + visOffset);
        }
    }
}