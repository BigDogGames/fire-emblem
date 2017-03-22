using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public string tileName = "Tile";
    public int defenseBonus = 0;
    public int avoidBonus = 0;
    public int healthBonus = 0;
    public int[] movementCosts;
    [HideInInspector]
    public List<Tile> neighbors;
    [HideInInspector]
    public Unit unit = null;
    // Use this for initialization
    public bool HasUnit() {
        return unit != null;
    }

    public int GetMoveCost( unitMovementType_t movementType ) {
        int val = (int)movementType;
        if ( val >= movementCosts.Length ) {
            print( "ERROR A TILE HAS A BAD MOVEMENT COSTS ARRAY" );
            return 1;
        }
        return movementCosts[ (int)movementType ];
    }
    // Update is called once per frame
    void Update () {
        
	}
}
