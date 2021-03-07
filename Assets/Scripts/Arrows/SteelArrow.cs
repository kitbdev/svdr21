using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SteelArrow : BaseArrowLogic
{
    protected override void Awake()
    {
        base.Awake();
        typeName = "Steel Arrow";
    }
}