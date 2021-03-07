using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Shapes;

public class GameManager : Singleton<GameManager>
{

    public SnapturnSingleHand snapturn;
    public XRRig xrRig;
    public Bow playerBow;
    public Transform player;

    public enum SnapTurnOptions
    {
        NONE, LEFTHAND, RIGHTHAND
    }
    private SnapTurnOptions m_snapTurnOptions;
    public SnapTurnOptions snapTurnOptions
    {
        get { return m_snapTurnOptions; }
        set { m_snapTurnOptions = value; UpdateSnapTurn(value); }
    }

    protected override void Awake()
    {
        base.Awake();
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
