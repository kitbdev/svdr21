using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using UnityEngine.XR.Interaction.Toolkit;

public class ArrowMenu : MonoBehaviour
{
    public Vector3 sideOffset = Vector3.right * 0.2f;
    public Vector3 displayLength = Vector3.up * 1f;
    // public Vector3 displaySpacing = Vector3.up * 0.2f;
    public float displayModelScale = 0.5f;
    public Vector3 displayRotation = Vector3.zero;
    public ArrowInteractable[] arrowPrefabs = new ArrowInteractable[0];

    public Vector3 displaySpacing => arrowPrefabs.Length == 1 ? Vector3.zero : displayLength / (arrowPrefabs.Length - 1);
    protected Vector3 dispStartPos => -displayLength / 2f;
    // protected Vector3 dispStartPos => displaySpacing * (-(arrowPrefabs.Length - 1) / 2f);

    protected List<ArrowInteractable> arrowsSpawned = new List<ArrowInteractable>();
    protected bool arrowsShown = false;
    public bool isOnRightSide = true;

    private void Awake()
    {
        arrowsShown = false;
    }
    public void SetSide(bool right)
    {
        isOnRightSide = right;
        sideOffset.x = Mathf.Abs(sideOffset.x) * (right ? 1 : -1);
        // update existing positions
        for (int i = 0; i < arrowsSpawned.Count; i++)
        {
            arrowsSpawned[i].transform.localPosition = DisplayModelPosFor(i);
        }
    }
    public void SpawnAllArrows()
    {
        for (int i = 0; i < arrowPrefabs.Length; i++)
        {
            SpawnArrowAt(i);
        }
    }
    public void DeSpawnAllArrows()
    {
        for (int i = arrowsSpawned.Count - 1; i >= 0; i--)
        {
            Destroy(arrowsSpawned[i]);
        }
    }
    protected void SpawnArrowAt(int i)
    {
        ArrowInteractable arrowPrefab = (ArrowInteractable)arrowPrefabs[i];
        var arrowGo = Instantiate(arrowPrefab, transform);
        arrowGo.name = arrowPrefab.name + "_" + i;
        arrowGo.transform.localPosition = DisplayModelPosFor(i);
        arrowGo.transform.localScale = displayModelScale * Vector3.one;
        arrowGo.transform.localRotation = Quaternion.Euler(displayRotation);
        ArrowInteractable arrow = arrowGo.GetComponent<ArrowInteractable>();
        arrow.selectEntered.AddListener((args) => { ArrowTaken(i, args); });
        arrowsSpawned.Insert(i, arrow);
    }
    protected Vector3 DisplayModelPosFor(int i)
    {
        return sideOffset + i * displaySpacing + dispStartPos;
    }
    public void ShowArrows()
    {
        if (arrowsSpawned.Count == 0)
        {
            SpawnAllArrows();
        } else
        {
            for (int i = 0; i < arrowsSpawned.Count; i++)
            {
                arrowsSpawned[i].transform.localScale = displayModelScale * Vector3.one;
                arrowsSpawned[i].gameObject.SetActive(true);
            }
        }
    }
    public void HoverArrow(int index)
    {
        // increase scale of that one
        // use dotween 
    }
    public void HideArrows()
    {
        for (int i = 0; i < arrowsSpawned.Count; i++)
        {
            arrowsSpawned[i].gameObject.SetActive(false);
            // arrowsSpawned[i].transform.localScale = 0.1f * Vector3.one;
        }
    }
    public void ArrowTaken(int index, SelectEnterEventArgs args)
    {

        VRDebug.Log("Chose arrow " + index);
        arrowsSpawned[index].transform.localScale = Vector3.one;
        // todo dont remove all listeners
        arrowsSpawned[index].selectEntered.RemoveAllListeners();
        arrowsSpawned.RemoveAt(index);
        SpawnArrowAt(index);
        HideArrows();
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < arrowPrefabs.Length; i++)
        {
            Vector3 pos = transform.position + DisplayModelPosFor(i);
            // Gizmos.DrawWireSphere(pos, 0.1f);
            Shapes.Draw.Sphere(pos, 0.02f, Color.cyan);
        }
    }
}