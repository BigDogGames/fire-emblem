using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum unitState_t : int {
    UNIT_STATE_IDLE,
    UNIT_STATE_SELECTED,
    UNIT_STATE_MOVING,
    UNIT_STATE_ATTACKING,
    UNIT_STATE_FINISHED
}

public enum unitTeam_t : int {
    UNIT_TEAM_PLAYER = 1,
    UNIT_TEAM_ENEMY = 2,
    UNIT_TEAM_OTHER = 4
}
public class Unit : MovingObject {

    public UnitClass unitClass;
    public GameObject moveRangeSprite;
    public GameObject attackRangeSprite;
    public unitTeam_t team = unitTeam_t.UNIT_TEAM_PLAYER;
    public string unitName = "Mike";

    public int startingLevel = 1;
    public int baseHP = 10;
    public int baseStr = 1;
    public int baseSpd = 1;

    [HideInInspector]
    public List<Vector2> movementRangeList;
    [HideInInspector]
    public List<Vector2> attackRangeList;

    private List<GameObject> rangeSpriteList;
    private unitState_t unitState;
    private List<Vector2> movePath;
	
    public string GetName() {
        return unitName;
    }
    public int GetMaxAttackRange() {
        return 1;
    }

    public int GetCurrentMovementRange() {
        if ( unitClass == null ) {
            print( "UNIT DOES NOT HAVE CLASS" );
            return 5;
        }
        return unitClass.movementRange;
    }

    public unitMovementType_t GetMovementType() {
        if ( unitClass == null ) {
            print( "UNIT DOES NOT HAVE CLASS" );
            return unitMovementType_t.MOVEMENT_FOOT;
        }
        return unitClass.movementType;
    }

    public void ShowRanges() {
        if ( unitState == unitState_t.UNIT_STATE_IDLE ) {
            unitState = unitState_t.UNIT_STATE_SELECTED;
            foreach ( Vector2 loc in movementRangeList ) {
                GameObject instance =
                                Instantiate( moveRangeSprite, new Vector3( loc.x, loc.y, 0f ), Quaternion.identity ) as GameObject;
                rangeSpriteList.Add( instance );
            }

            foreach ( Vector2 loc in attackRangeList ) {
                GameObject instance =
                                Instantiate( attackRangeSprite, new Vector3( loc.x, loc.y, 0f ), Quaternion.identity ) as GameObject;
                rangeSpriteList.Add( instance );
            }
        }
    }

    public void HideRanges() {
        if ( unitState == unitState_t.UNIT_STATE_SELECTED ) {
            unitState = unitState_t.UNIT_STATE_IDLE;
            foreach ( GameObject sprite in rangeSpriteList ) {
                DestroyObject( sprite );
            }
            rangeSpriteList.Clear();
        }
    }

    void AttemptNextMove() {
        //Hit will store whatever our linecast hits when Move is called.
        if ( movePath.Count > 0 ) {
            Vector2 nextMove = movePath[ 0 ];
            movePath.RemoveAt( 0 );
                MoveTo( nextMove);
        }
    }

    public void SetMovement( List< Vector2 > path ) {
        movePath = path;
        AttemptNextMove();
    }

    protected override void FinishedMoving() {
        if ( movePath.Count > 0 ) {
            AttemptNextMove();
        } else {
            isMoving = false;
            GameManager.instance.ShowActionMenu();
        }
    }

    protected override void Start() {
        base.Start();
        rangeSpriteList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
