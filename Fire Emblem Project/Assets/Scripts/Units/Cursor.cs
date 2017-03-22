using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MovingObject {

    public GameObject trailArrow;
    public GameObject trailStraight;
    public GameObject trailTurn;
    private Unit currentUnit = null;
    private List<GameObject> movementTrail;
    private BoardManager boardManager;
    private bool offRange = false;

    protected override void Start() {
        base.Start();
        movementTrail = new List<GameObject>();
        boardManager = BoardManager.instance;
    }
    protected override void AttemptMove( int xDir, int yDir ) {
        //Hit will store whatever our linecast hits when Move is called.
        if ( !isMoving ) {
            if ( boardManager.CanMoveVec2( transform.position, xDir, yDir ) ) {
                Move( xDir, yDir );
            }
        }
    }

    // Update is called once per frame
    private void Update() {
        //If it's not the player's turn, exit the function.
        if ( !GameManager.instance.playersTurn ) return;

        bool isPressingSelected = Input.GetButtonDown( "Jump" );
        if  ( isPressingSelected ) {
            if ( currentUnit == null ) {
                BoardManager boardManager = BoardManager.instance;
                if ( boardManager != null ) {
                    Unit newUnit = boardManager.GetCurrentUnit();
                    if ( newUnit == null ) {
                        print( "No unit" );
                    } else {
                        currentUnit = newUnit;
                        currentUnit.ShowRanges();
                    }
                }
            } else if ( currentUnit.team == unitTeam_t.UNIT_TEAM_PLAYER ) {
                if ( IsOnMoveRange() ) {
                    List<Vector2> unitPath = new List<Vector2>();
                    for ( int i = 1; i < movementTrail.Count; ++i ) {
                        unitPath.Add( movementTrail[ i ].transform.position );
                    }
                    currentUnit.SetMovement( unitPath );
                }
            } 
        } else {
            bool isCancelSelected = Input.GetButtonDown( "Cancel" );
            if ( isCancelSelected ) {
                if ( currentUnit == null ) {
                    print( "No unit" );
                } else {
                    DestroyTrail();
                    currentUnit.HideRanges();
                }
            }
        }

        int horizontal = (int)( Input.GetAxisRaw( "Horizontal" ) );
        int vertical = (int)( Input.GetAxisRaw( "Vertical" ) );

        //Check if moving horizontally, if so set vertical to zero.
        if ( horizontal != 0 ) {
            vertical = 0;
        } else if ( vertical != 0 ) {
            horizontal = 0;
        }

        //Check if we have a non-zero value for horizontal or vertical
        if ( horizontal != 0 || vertical != 0 ) {
            AttemptMove( horizontal, vertical );
        }
    }

    bool IsOnMoveRange() {
        if ( currentUnit == null ) {
            return false;
        }
        Vector2 currentPos = transform.position;
        foreach ( Vector2 pos in currentUnit.movementRangeList ) {
            if ( pos == currentPos ) {
                return true;
            }
        }
        return false;
    }

    protected override void FinishedMoving() {
        base.FinishedMoving();
        if ( currentUnit != null ) {
            if ( IsOnMoveRange() ) {
                if ( offRange ) {
                    offRange = false;
                    DestroyTrail();
                    List<int> path = new List<int>();
                    boardManager.FindShortestPath( currentUnit, transform.position, path );
                    for ( int i = path.Count - 2; i >= 0; --i ) {
                        SetTrail( boardManager.GetPositionFromIndex( path[ i ] ) );
                    }
                } else {
                    SetTrail();
                }
                
            } else {
                offRange = true;
            }
            
        } 
    }

    Direction GetDirectionFromPreviousTrail( Vector3 curPos, Vector3 startPos ) {
        Vector3 dif = curPos - startPos;
        return BoardManager.GetDirection( dif );
    }

    float GetAngleFromDirection( Direction dir ) {
        float angle = 0;
        if ( dir == Direction.EAST ) {
            angle = -90;
        } else if ( dir == Direction.WEST ) {
            angle = 90;
        } else if ( dir == Direction.SOUTH ) {
            angle = 180;
        }

        return angle;
    }

    Direction GetDirectionFromAngle( float angle ) {
        Direction dir = Direction.INVALID;
        if ( angle == 270 ) {
            dir = Direction.EAST;
        } else if ( angle == 90 ) {
            dir = Direction.WEST;
        } else if ( angle == 180 ) {
            dir = Direction.SOUTH;
        } else if ( angle == 0 ) {
            dir = Direction.NORTH;
        }
        return dir;
    }
    void SetTrail( Vector3 currentLoc ) {
        Vector3 startPos;
        if ( movementTrail.Count == 0 ) {
            startPos = currentUnit.transform.position;
            Direction dir = GetDirectionFromPreviousTrail( currentLoc, startPos );
            GameObject instanceStraight =
                                Instantiate( trailStraight, new Vector3( startPos.x, startPos.y, 0f ), Quaternion.identity ) as GameObject;
            GameObject instanceArrow =
                                Instantiate( trailArrow, new Vector3( currentLoc.x, currentLoc.y, 0f ), Quaternion.identity ) as GameObject;
            float angle = GetAngleFromDirection( dir );
            instanceStraight.transform.rotation = Quaternion.AngleAxis( angle, Vector3.forward );
            instanceArrow.transform.rotation = Quaternion.AngleAxis( angle, Vector3.forward );
            movementTrail.Add( instanceStraight );
            movementTrail.Add( instanceArrow );
        } else if ( movementTrail.Count <= currentUnit.GetCurrentMovementRange() ) {
            GameObject trailArrow = movementTrail[ movementTrail.Count - 1 ];
            startPos = trailArrow.transform.position;
            Direction dir = GetDirectionFromPreviousTrail( currentLoc, startPos );
            float arrowAngle = trailArrow.transform.rotation.eulerAngles.z;
            Direction arrowDir = GetDirectionFromAngle( arrowAngle );
            GameObject replacementType = null;
            if ( arrowDir == dir ) {
                replacementType = trailStraight;
            } else if ( !BoardManager.AreDirectionsOpposite( dir, arrowDir ) ) {
                replacementType = trailTurn;
            }
            //movementTrail.RemoveAt( movementTrail.Count - 1 );
            if ( replacementType == null ) {
                if ( movementTrail.Count >= 3 ) {
                    arrowAngle = movementTrail[ movementTrail.Count - 3 ].transform.eulerAngles.z;
                }
                GameObject objectToDestroy = movementTrail[ movementTrail.Count - 2 ];
                DestroyObject( objectToDestroy );
                movementTrail.RemoveAt( movementTrail.Count - 2 );
                if ( movementTrail.Count == 1 ) {
                    GameObject trailStart = movementTrail[ 0 ];
                    DestroyObject( trailStart );
                    movementTrail.RemoveAt( 0 );
                }

                //movementTrail.Add( trailArrow );
            } else {
                GameObject instance =
                                Instantiate( replacementType, new Vector3( startPos.x, startPos.y, 0f ), Quaternion.identity ) as GameObject;

                float angle = GetAngleFromDirection( dir );

                instance.transform.rotation = Quaternion.AngleAxis( angle, Vector3.forward );
                if ( replacementType == trailTurn ) {
                    SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
                    if ( sr != null ) {
                        if ( angle - arrowAngle == -90 ) {
                            sr.flipX = true;
                        }
                    }
                }
                arrowAngle = angle;
                movementTrail.Insert( movementTrail.Count - 1, instance );
            }
            trailArrow.transform.position = currentLoc;
            //trailArrow.transform.rotation = Quaternion.AngleAxis( angle, Vector3.forward );
            trailArrow.transform.eulerAngles = new Vector3( 0, 0, arrowAngle );
        } else {
            DestroyTrail();
            List<int> path = new List<int>();
            boardManager.FindShortestPath( currentUnit, currentLoc, path );
            for ( int i = path.Count - 2; i >= 0; --i ) {
                SetTrail( boardManager.GetPositionFromIndex( path[ i ] ) );
            }
        }
    }

    void SetTrail() {
        Vector3 currentLoc = transform.position;
        if ( currentLoc == currentUnit.transform.position ) {
            DestroyTrail();
        } else {
            SetTrail( currentLoc );
        }
    }

    public void DestroyTrail() {
        foreach ( GameObject sprite in movementTrail ) {
            DestroyObject( sprite );
        }
        movementTrail.Clear();
    }
}


