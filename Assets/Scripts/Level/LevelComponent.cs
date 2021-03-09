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
    public List<LevelComponent> onlyIf = new List<LevelComponent>();
    public GameObject activeGO;
    public GameObject replacementGO;
    [ReadOnly] public bool isInUse = false;
    [ReadOnly] public Room myRoom;

    public void SetUsing(bool isUsing)
    {
        isInUse = isUsing;
        if (activeGO)
        {
            activeGO.SetActive(isUsing);
        } else
        {
            gameObject.SetActive(isUsing);
        }
        if (replacementGO)
        {
            replacementGO.SetActive(!isUsing);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (isRoomConnector)
        {
            Draw.Color = Color.white;
            float size = 1.5f;
            Vector3 endPos = transform.position + transform.forward * 0.3f * size;
            Draw.Line(transform.position, endPos);
            Draw.Cone(endPos, transform.forward, 0.1f * size, 0.1f * size);
        }
    }
}
