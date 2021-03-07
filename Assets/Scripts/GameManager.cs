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

    protected override void Awake()
    {
        base.Awake();
        if (!snapturn) snapturn = GameObject.FindObjectOfType<SnapturnSingleHand>();
    }

    public void StartNewLevel()
    {
        // LevelGen.Instance
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
