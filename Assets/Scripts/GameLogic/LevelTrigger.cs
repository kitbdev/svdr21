using UnityEngine;

/// <summary>
/// calls misc level manager stuff from trigger
/// </summary>
[RequireComponent(typeof(Trigger))]
public class LevelTrigger : MonoBehaviour
{
    Trigger trigger;
 
    private void Awake()
    {
        trigger = GetComponentInChildren<Trigger>();
    }
    public void EnterEndRoom()
    {
        LevelManager.Instance.LevelComplete();
    }
    public void LeaveEndRoom() {

    }
    public void LeaveMainRoom() {

    }

}