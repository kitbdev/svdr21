using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOptionalComponent : MonoBehaviour
{
    public bool isRoomConnector = false;
    public List<LevelOptionalComponent> requireOneOf = new List<LevelOptionalComponent>();
    public List<LevelOptionalComponent> blocks = new List<LevelOptionalComponent>();
    public List<LevelOptionalComponent> onlyIf = new List<LevelOptionalComponent>();
}
