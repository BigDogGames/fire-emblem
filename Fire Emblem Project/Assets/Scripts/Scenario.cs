using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum objectiveType_t {
    OBJECTIVE_TYPE_ROUT,
    OBJECTIVE_TYPE_SEIZE
}

public class Scenario : MonoBehaviour {
    [Serializable]
    public struct enemySpawn_t {
        public Unit enemy;
        public Vector2 spawnLoc;
        public int level;
    }

    public objectiveType_t objective = objectiveType_t.OBJECTIVE_TYPE_ROUT;
    public int unitsAllowed = 1;
    public Vector2[] playerSpawns;
    public enemySpawn_t[] enemyList;

    private void OnValidate() {
        if ( playerSpawns.Length != unitsAllowed ) {
            Debug.LogWarning( "playerSpawns is not equal to units allowed!!" );
            System.Array.Resize( ref playerSpawns, unitsAllowed );
        }
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
