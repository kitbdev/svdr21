using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Shapes;

public class GameManager : Singleton<GameManager>
{
    public const string PlayerTag = "Player";
    public SnapturnSingleHand snapturn;
    public XRRig xrRig;
    public Bow playerBow;
    public Transform player;

    public enum SnapTurnOptions
    {
        NONE, LEFTHAND, RIGHTHAND, BOTH, PRIMARYHAND, OFFHAND
    }
    [SerializeField]
    private SnapTurnOptions m_snapTurnOptions = SnapTurnOptions.BOTH;
    public SnapTurnOptions snapTurnOptions
    {
        get { return m_snapTurnOptions; }
        set { m_snapTurnOptions = value; UpdateSnapTurn(value); }
    }

    protected override void Awake()
    {
        base.Awake();
        if (!playerBow) playerBow = GameObject.FindObjectOfType<Bow>();
        if (!snapturn) snapturn = GameObject.FindObjectOfType<SnapturnSingleHand>();
        SetSnapTurn(snapTurnOptions);
    }

    public void StartNewLevel()
    {
        // LevelGen.Instance
    }
    private void UpdateSnapTurn(SnapTurnOptions options)
    {
        if (!snapturn)
        {
            return;
        }
        if (options == SnapTurnOptions.NONE)
        {
            snapturn.turnLeftEnabled = false;
            snapturn.turnRightEnabled = false;
        } else if (options == SnapTurnOptions.LEFTHAND)
        {
            snapturn.turnLeftEnabled = true;
            snapturn.turnRightEnabled = false;
        } else if (options == SnapTurnOptions.RIGHTHAND)
        {
            snapturn.turnLeftEnabled = false;
            snapturn.turnRightEnabled = true;
        } else if (options == SnapTurnOptions.BOTH)
        {
            snapturn.turnLeftEnabled = true;
            snapturn.turnRightEnabled = true;
        } else if (options == SnapTurnOptions.PRIMARYHAND)
        {
            if (playerBow.primaryLeftHand)
            {
                snapturn.turnLeftEnabled = true;
                snapturn.turnRightEnabled = false;
            } else
            {
                snapturn.turnLeftEnabled = false;
                snapturn.turnRightEnabled = true;
            }
        } else if (options == SnapTurnOptions.OFFHAND)
        {
            if (playerBow.primaryLeftHand)
            {
                snapturn.turnLeftEnabled = false;
                snapturn.turnRightEnabled = true;
            } else
            {
                snapturn.turnLeftEnabled = true;
                snapturn.turnRightEnabled = false;
            }
        }
    }
    public void SetSnapTurn(SnapTurnOptions options)
    {
        snapTurnOptions = options;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
