using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// manages Items
/// </summary>
public class Inventory : MonoBehaviour
{
    public int numKeys { get; protected set; }
    public int numGems { get; protected set; }

    enum ArrowType
    {
        Steel, Tele, Explosive,
    }
    public int[] numArrows = new int[4] {
        0,0,0,0
    };
    public UnityEvent inventoryModifiedEvent;


    private void Awake()
    {

    }
    private void OnEnable()
    {
        LevelManager.Instance.restartReadyEvent.AddListener(OnLevelReload);
    }
    private void OnDisable()
    {
        LevelManager.Instance?.restartReadyEvent.RemoveListener(OnLevelReload);
    }
    private void Start()
    {
        ResetInventory();
        // todo load
    }
    public void ResetInventory()
    {
        ResetKeys();
        numGems = 0;
        UpdateInventory();
    }
    public void ResetKeys()
    {
        numKeys = 0;
        UpdateInventory();
    }
    void OnLevelReload()
    {
        // reset keys but not gems on level reload
        ResetKeys();
    }
    public void AddKey(int amount = 1)
    {
        numKeys += amount;
        UpdateInventory();
    }
    public void RemoveKey(int amount = 1)
    {
        numKeys -= amount;
        UpdateInventory();
    }
    public void AddGem(int amount = 1)
    {
        numGems += amount;
        UpdateInventory();
    }
    public void RemoveGem(int amount = 1)
    {
        numGems -= amount;
        UpdateInventory();
    }
    public void AddArrow()
    {

    }
    public void RemoveArrow()
    {

    }
    void UpdateInventory()
    {
        inventoryModifiedEvent.Invoke();
    }
    /*
    keeps track of
    gems - cant use, currency automatically handled when purchasing
    arrows - cant use, bow handles it
    potions
    keys
    
    how will it be accessed?
    player needs to grab an item
    */
}