using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum unitMovementType_t {
    MOVEMENT_FOOT = 1,
    MOVEMENT_MOUNT = 2,
    MOVEMENT_FLIER = 3,
    NUM_MOVEMENT_OPTIONS
}

public class UnitClass : MonoBehaviour {

    public string className = "Soldier";
    public int movementRange = 5;
    public unitMovementType_t movementType = unitMovementType_t.MOVEMENT_FOOT;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}

