using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    // todo seperate portal script?

    // todo slowly, after some time
    public float warmUpDur = 0;
    Trigger trigger;
    // todo anim

    private void Awake()
    {
        trigger = GetComponentInChildren<Trigger>();
        trigger.triggerEnteredEvent.AddListener(OnLevelEnd);
    }
    private void OnDisable() {
        trigger.triggerEnteredEvent.RemoveListener(OnLevelEnd);
    }
    public void OnLevelEnd()
    {
        LevelManager.Instance.LevelComplete();
    }

}