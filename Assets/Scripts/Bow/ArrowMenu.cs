using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Shows menu next to the bow
/// selection from input
/// arrows are real and can be taken by player
/// </summary>
public class ArrowMenu : MonoBehaviour
{
    public Vector3 sideOffset = Vector3.right * 0.2f;
    public Vector3 displayLength = Vector3.up * 1f;
    // public Vector3 displaySpacing = Vector3.up * 0.2f;
    public float displayModelScale = 0.05f;
    public float displayModelArrowScale = 0.5f;
    public Vector3 displayRotation = Vector3.zero;
    public float displaySpacer = 0.2f;
    public float displaySelectedScaleMod = 2;
    public Vector3 displaySpacing => arrowPrefabs.Length == 1 ? Vector3.zero : displayLength / (arrowPrefabs.Length - 1);
    protected Vector3 dispStartPos => -displayLength / 2f;
    // protected Vector3 dispStartPos => displaySpacing * (-(arrowPrefabs.Length - 1) / 2f);

    public ArrowInteractable[] arrowPrefabs = new ArrowInteractable[0];
    public GameObject menuItemPrefab;
    public GameObject deactivatedMenuItemPrefab;// or a setting on it?

    [ReadOnly] [SerializeField] protected List<GameObject> menuItems = new List<GameObject>();
    [ReadOnly] [SerializeField] protected List<ArrowInteractable> arrowsSpawned = new List<ArrowInteractable>();
    [ReadOnly] [SerializeField] protected bool arrowsShown = false;
    [ReadOnly] [SerializeField] protected int curSel = 0;
    public bool isOnRightSide = true;
    [Header("Anim")]
    public float scaleDur = 0.2f;
    protected Bow bow;

    XRControls xrControls;
    Vector2 inputSelect = Vector2.zero;

    // List<Vector2> dirOrder4 = new List<Vector2>() { Vector2.zero, Vector2.up, Vector2.down, Vector2.right, Vector2.left };
    List<Vector2> dirOrder8 = new List<Vector2>() {
        Vector2.zero, Vector2.up, Vector2.down, Vector2.right, Vector2.left,
        new Vector2(1,1),new Vector2(1,-1),new Vector2(-1,-1),new Vector2(-1,1),
        };

    private void Awake()
    {
        arrowsShown = false;
        xrControls = new XRControls();
        xrControls.Enable();
        bow = GetComponentInParent<Bow>();
        // todo left/right primary issues
        xrControls.PrimaryHand.SelectMove.performed += c => { inputSelect = c.ReadValue<Vector2>(); UpdateSel(); };
        xrControls.PrimaryHand.SelectMove.canceled += c => { inputSelect = Vector2.zero; UpdateSel(); };
        curSel = 0;
    }
    public void SetSide(bool right)
    {
        isOnRightSide = right;
        sideOffset.x = Mathf.Abs(sideOffset.x) * (right ? 1 : -1);
        // update existing positions
        UpdateMenuPositions();
    }
    public void SpawnAllArrows()
    {
        if (bow.debugLog) VRDebug.Log("Spawning all arrows");
        for (int i = 0; i < arrowPrefabs.Length; i++)
        {
            CreateMenuItem(i);
        }
    }
    public void DeSpawnAllArrows()
    {
        if (bow.debugLog) VRDebug.Log("Despawning all arrows");
        for (int i = menuItems.Count - 1; i >= 0; i--)
        {
            Destroy(menuItems[i]);
        }
        menuItems.Clear();
        arrowsSpawned.Clear();
    }
    public ArrowInteractable GetArrow(int i)
    {
        return arrowsSpawned[i];
    }
    public void UpdateMenuPositions()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            menuItems[i].transform.localPosition = DisplayModelPosFor(i);
        }
        // ? also scale?
    }
    // todo set color to disabled when bow canuse is false
    void UpdateSel()
    {
        // GetSelectedArrow();
        int selArrow = selectDirToInt();
        if (selArrow == curSel)
        {
            // already selecting that
            return;
        }
        Debug.Log("UpdatingSel");
        // todo change arrow immediately

        // update scale
        for (int i = 0; i < menuItems.Count; i++)
        {
            GameObject menuitem = menuItems[i];
            float scale = displayModelScale;
            if (i == selArrow)
            {
                scale *= displaySelectedScaleMod;
            }
            // menuitem.transform.localScale = scale * Vector3.one;
            menuitem.transform.DOScale(scale, scaleDur);
        }
        curSel = selArrow;
    }

    int selectDirToInt()
    {
        Vector2 normalizedSelDir = inputSelect;
        int selInd = 0;
        if (inputSelect.magnitude <= 0.1f)
        {
            normalizedSelDir = Vector2.zero;
            selInd = 0;
        } else
        {
            normalizedSelDir.Normalize();
            // get closest
            int closestInt = 0;
            float closestAmount = 10;
            int numDirs = Mathf.Min(dirOrder8.Count, menuItems.Count);
            for (int i = 0; i < numDirs; i++)
            {
                Vector2 dirO = dirOrder8[i];
                float dist = Vector2.Distance(dirO, normalizedSelDir);
                if (dist < closestAmount)
                {
                    closestAmount = dist;
                    closestInt = i;
                }
            }
            selInd = closestInt;
        }
        // selInd = Mathf.Clamp(selInd, 0, arrowsSpawned.Count);
        return selInd;
    }
    public ArrowInteractable GetSelectedArrow()
    {
        return arrowsSpawned[curSel];
    }
    protected void CreateMenuItem(int i)
    {
        // menu
        var menuItemgo = Instantiate(menuItemPrefab, transform);
        menuItems.Insert(i, menuItemgo);
        // arrow
        ArrowInteractable arrowPrefab = (ArrowInteractable)arrowPrefabs[i];
        var arrowGo = Instantiate(arrowPrefab, menuItemgo.transform);
        arrowGo.name = arrowPrefab.name + "_" + i;
        arrowGo.transform.localPosition = Vector3.zero;
        arrowGo.transform.localScale = Vector3.one * displayModelArrowScale / displayModelScale;
        arrowGo.transform.localRotation = Quaternion.Euler(displayRotation);
        ArrowInteractable arrow = arrowGo.GetComponent<ArrowInteractable>();
        arrow.ArrowDisplay(true);
        arrow.selectEntered.AddListener((args) => { ArrowTaken(i, args); });
        arrowsSpawned.Insert(i, arrow);

        UpdateMenuPositions();
        UpdateSel();
    }
    protected Vector3 DisplayModelPosFor(int i)
    {
        // linear
        // Vector3 pos = sideOffset + i * displaySpacing + dispStartPos;
        // plus menu
        Vector2 flatdir = dirOrder8[i];
        Vector3 dir = new Vector3(flatdir.x, flatdir.y, 0);
        Vector3 pos = sideOffset + dir * displaySpacer;
        return pos;
    }
    public void ShowMenu()
    {
        if (menuItems.Count == 0)
        {
            SpawnAllArrows();
        } else
        {
            if (bow.debugLog) VRDebug.Log("Showing Arrows");
            for (int i = 0; i < menuItems.Count; i++)
            {
                // menuItems[i].transform.localScale = displayModelScale * Vector3.one;
                menuItems[i].gameObject.SetActive(true);
            }
            UpdateSel();
        }
    }
    public void HoverArrow(int index)
    {
        // ?increase scale of that one
        // todo use dotween 
    }
    public void HideMenuItems()
    {
        if (bow.debugLog) VRDebug.Log("Hiding Arrows");
        for (int i = 0; i < menuItems.Count; i++)
        {
            menuItems[i].transform.DOKill();
            menuItems[i].gameObject.SetActive(false);
            // menuItems[i].transform.localScale = 0.1f * Vector3.one;
        }
    }
    public void ArrowTaken(int index, SelectEnterEventArgs args)
    {
        if (bow.debugLog) VRDebug.Log("Arrow taken " + index);
        arrowsSpawned[index].transform.localScale = Vector3.one;
        int killed = menuItems[index].transform.DOKill();
        killed += DOTween.Kill(menuItems[index]);
        if (bow.debugLog) Debug.Log("select killed " + killed);
        // ? todo dont remove all listeners
        arrowsSpawned[index].selectEntered.RemoveAllListeners();
        // make sure arrow is already unparented
        if (arrowsSpawned[index].transform.parent == menuItems[index].transform)
        {
            VRDebug.Log("Arrow taken not unparented!");
            arrowsSpawned[index].transform.SetParent(null);
        }
        // remove the menu item
        Destroy(menuItems[index]);
        menuItems.RemoveAt(index);
        arrowsSpawned.RemoveAt(index);
        // recreate it, with a new arrow
        CreateMenuItem(index);
        // disable menu for now
        HideMenuItems();
    }
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            return;
        }
        for (int i = 0; i < arrowPrefabs.Length; i++)
        {
            Vector3 pos = transform.position + DisplayModelPosFor(i);
            // Gizmos.DrawWireSphere(pos, 0.1f);
            Shapes.Draw.Sphere(pos, 0.02f, Color.cyan);
        }
    }
}