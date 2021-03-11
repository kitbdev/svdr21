using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// adds an item to player inventory
/// </summary>
public class DropItem : MonoBehaviour
{
    public bool giveGem = false;
    public bool giveKey = false;
    // public bool[] numArrows = new bool[0];

    public void GiveToPlayer()
    {
        if (giveGem)
        {
            GameManager.Instance.playerInventory.AddGem();
        }
        if (giveKey)
        {
            GameManager.Instance.playerInventory.AddKey();
        }
    }
}