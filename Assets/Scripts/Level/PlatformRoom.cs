using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformRoom : MonoBehaviour {
    Room myRoom;
    private void Awake() {
        myRoom = GetComponent<Room>();
    }
    private void OnEnable() {
        myRoom.roomStartEvent.AddListener(Setup);
    }
    private void OnDisable() {
        myRoom.roomStartEvent.RemoveListener(Setup);
    }
    void Setup() {

    }
}