using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapturnSingleHand : ActionBasedSnapTurnProvider
{
    [SerializeField]
    protected bool m_turnRightEnabled = true;
    public bool turnRightEnabled
    {
        get {
            return m_turnRightEnabled;
        }
        set {
            m_turnRightEnabled = value;
            if (value)
            {
                rightHandSnapTurnAction.action.Enable();
            } else
            {
                rightHandSnapTurnAction.action.Disable();
            }
        }
    }
    [SerializeField]
    protected bool m_turnLeftEnabled = true;
    public bool turnLeftEnabled
    {
        get {
            return m_turnLeftEnabled;
        }
        set {
            m_turnLeftEnabled = value;
            if (value)
            {
                leftHandSnapTurnAction.action.Enable();
            } else
            {
                leftHandSnapTurnAction.action.Disable();
            }
        }
    }
}