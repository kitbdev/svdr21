using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapturnSingleHand : ActionBasedSnapTurnProvider
{

    public bool turnRightEnabled = true;
    public bool turnLeftEnabled = true;

    protected override Vector2 ReadInput()
    {
        // return base.ReadInput();
        var leftHandValue = leftHandSnapTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
        var rightHandValue = rightHandSnapTurnAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
        if (!turnLeftEnabled)
        {
            leftHandValue = Vector2.zero;
        }
        if (!turnRightEnabled)
        {
            rightHandValue = Vector2.zero;
        }
        // ? kinda working?

        return leftHandValue + rightHandValue;
    }
}