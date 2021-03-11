using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using TMPro;

/// <summary>
/// shows the inventory
/// </summary>
public class InventoryVis : MonoBehaviour
{
    public TMP_Text keyText;
    public TMP_Text gemText;
    public TMP_Text[] arrowTexts = new TMP_Text[0];

    Inventory inv;
    private void OnEnable()
    {
        inv = GameManager.Instance.playerInventory;
        inv.inventoryModifiedEvent.AddListener(UpdateText);
    }
    private void OnDisable()
    {
        inv?.inventoryModifiedEvent.RemoveListener(UpdateText);
    }
    public void UpdateText()
    {
        keyText.text = inv.numKeys + "";
        gemText.text = inv.numGems + "";
        // todo arrows
    }
}