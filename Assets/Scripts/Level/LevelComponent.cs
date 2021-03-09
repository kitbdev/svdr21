using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class LevelComponent : MonoBehaviour
{
    public bool isRoomConnector = false;
    public bool isRequired = false;
    public List<LevelComponent> requireOneOf = new List<LevelComponent>();
    public List<LevelComponent> blocks = new List<LevelComponent>();
    public List<LevelComponent> forceUse = new List<LevelComponent>();
    public GameObject[] activeGOs;
    public GameObject[] replacementGOs;
    [ReadOnly] public bool isInUse = false;
    [ReadOnly] public Room myRoom;

    public void SetUsing(bool isUsing)
    {
        isInUse = isUsing;
        if (activeGOs.Length > 0)
        {
            foreach (var activeGO in activeGOs)
            {
                activeGO.SetActive(isUsing);
            }
        } else
        {
            gameObject.SetActive(isUsing);
        }
        foreach (var replacementGO in replacementGOs)
        {
            replacementGO.SetActive(!isUsing);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (isRoomConnector)
        {
            Draw.ZTest = UnityEngine.Rendering.CompareFunction.Always;
            Draw.Color = Color.white;
            float size = 1.5f;
            Vector3 endPos = transform.position + transform.forward * 0.3f * size;
            Draw.Line(transform.position, endPos);
            Draw.Cone(endPos, transform.forward, 0.1f * size, 0.1f * size);
        }
    }
}
