using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class LevelOptionalComponent : MonoBehaviour
{
    public bool isRoomConnector = false;
    public List<LevelOptionalComponent> requireOneOf = new List<LevelOptionalComponent>();
    public List<LevelOptionalComponent> blocks = new List<LevelOptionalComponent>();
    public List<LevelOptionalComponent> onlyIf = new List<LevelOptionalComponent>();

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
