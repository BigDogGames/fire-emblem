using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Direction : int { INVALID = 0, NORTH = 1, EAST = 2, SOUTH = -1, WEST = -2 };



public class BoardManager : MonoBehaviour {

    public static BoardManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

    Direction[] cardinalDirections = { Direction.NORTH, Direction.EAST, Direction.SOUTH, Direction.WEST };
    [Serializable]
    public class Count {
        public int minimum;
        public int maximum;

        public Count( int min, int max ) {
            minimum = min;
            maximum = max;
        }
    }

    public int columns = 8;
    public int rows = 8;
    public Count wallCount = new Count(5, 9);
    public GameObject cursor;
    public GameObject[] floorTiles;
    public GameObject[] enemyTiles;
    public Scenario scenario = null;
    public Unit player = null;

    private Transform boardHolder;
    private List<Vector3> gridPositions = new List<Vector3>();
    private GameObject cursorInstance = null;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> enemyUnits = new List<Unit>();
    private List<Unit> otherUnits = new List<Unit>();

    //assumes normalized dir
    public static Direction GetDirection( Vector3 dir ) {
        dir.Normalize();
        if ( dir.x == 1 ) {
            return Direction.EAST;
        } else if ( dir.x == -1 ) {
            return Direction.WEST;
        } else if ( dir.y == 1 ) {
            return Direction.NORTH;
        } else if ( dir.y == -1 ) {
            return Direction.SOUTH;
        }

        return Direction.INVALID;
    }

    public static bool AreDirectionsOpposite( Direction dir1, Direction dir2 ) {
        return (int)dir1 + (int)dir2 == 0 ;
    }

    void InitialiseList() {
        gridPositions.Clear();
        for ( int x = 0; x < columns; ++x ) {
            for ( int y = 0; y < rows; ++y ) {
                gridPositions.Add( new Vector3( x, y, 0f ) );
            }
        }
    }

    private void Awake() {
            //Check if instance already exists
            if ( instance == null ) {

                //if not, set instance to this
                instance = this;
            }
            //If instance already exists and it's not this:
            else if ( instance != this ) {

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy( gameObject );
            }

            
            if ( GameManager.instance != null ) {
            GameManager.instance.SetBoardManager( this );
        }
    } 

    public int GetPositionIndex( int x, int y ) {
        return x + y * columns;
    }

    public int GetPositionIndex( float x, float y ) {
        return (int)x + (int)y * columns;
    }

    public int GetPositionIndex( Vector2 pos ) {
        return GetPositionIndex( (int)pos.x, (int)pos.y );
    }

    public Vector2 GetPositionFromIndex( int index ) {
        Vector2 newVec;
        newVec.y = index / columns;
        newVec.x = index % columns; ;
        return newVec;
    }

    bool isValid( Vector2 loc ) {
        return ( loc.x >= 0 && loc.x < columns && loc.y >= 0 && loc.y < rows );
    }

    bool isValid( int index ) {
        return ( index >= 0 && index < floorTiles.Length );
    }

    void AddTileNeighbor( Tile currentTile, int currentIndex, Tile prevTile = null ) {
        if ( prevTile != null && !currentTile.neighbors.Contains( prevTile ) ) {
            currentTile.neighbors.Add( prevTile );
        }

        foreach ( Direction dir in cardinalDirections ) {
            int newIndex = MoveIndexByDirection( currentIndex, dir );
            if ( newIndex >= 0 ) {
                Tile newNode = floorTiles[ newIndex ].GetComponent<Tile>();
                if ( newNode != null && !currentTile.neighbors.Contains( newNode ) ) {
                    currentTile.neighbors.Add( newNode );
                    AddTileNeighbor( newNode, newIndex, currentTile );
                }
            }
        }
    }

    void BoardSetup() {
        //Instantiate Board and set boardHolder to its transform.
        boardHolder = new GameObject( "Board" ).transform;

        cursorInstance = Instantiate( cursor, new Vector3( 0f, 0f, 0f ), Quaternion.identity ) as GameObject;
        cursorInstance.transform.SetParent( boardHolder );
        if( floorTiles.Length > 0 ) {
            Tile startTile = floorTiles[ 0 ].GetComponent<Tile>();
            if ( startTile != null ) {
                AddTileNeighbor( startTile, 0 );
            }
        }
        
    }

    void UnitSetup() {
        Unit playerInstance = Instantiate( player, new Vector3( 0f, 0f, 0f ), Quaternion.identity ) as Unit;
        playerInstance.transform.SetParent( boardHolder );
        playerUnits.Add( playerInstance );
        int floorIndex = GetPositionIndex( playerInstance.transform.position );
        floorTiles[ floorIndex ].GetComponent<Tile>().unit = playerInstance;

        CalculateRanges( playerInstance );

        if ( scenario != null ) {
            foreach ( Scenario.enemySpawn_t enemySpawn in scenario.enemyList ) {
                Unit enemyInstance = Instantiate( enemySpawn.enemy, enemySpawn.spawnLoc, Quaternion.identity ) as Unit;
                enemyInstance.transform.SetParent( boardHolder );
                enemyUnits.Add( enemyInstance );
                floorIndex = GetPositionIndex( enemyInstance.transform.position );
                floorTiles[ floorIndex ].GetComponent<Tile>().unit = enemyInstance;
            }
        }
    }
    
    void AddNode( int newIndex, List<int> frontier, int[] moveCost, int range, Unit unit ) {
        if ( moveCost[ newIndex ] < 0 ) {
            Tile tile = GetTileByIndex( newIndex );
            if ( tile != null ) {
                moveCost[ newIndex ] = range + tile.GetMoveCost( unit.GetMovementType() );
                frontier.Add( newIndex );
            }
        }
    }

    void AddAttackNode( int newIndex, List<int> frontier, int[] moveCost, int[] attackMoveCost, int range ) {
        if ( moveCost[ newIndex ] < 0 && attackMoveCost[ newIndex ] <= 0 ) {
            attackMoveCost[ newIndex ] = range + 1;
            frontier.Add( newIndex );
        }
    }
    public int MoveIndexByDirection( int index, Direction dir ) {
        if ( dir == Direction.EAST ) {
            if ( index % columns != columns - 1 ) {
                return index + 1;
            }
        } else if ( dir == Direction.WEST ) {
            if ( index % columns != 0 ) {
                return index - 1; ;
            }
        } else if ( dir == Direction.NORTH ) {
           int newIndex = index + columns;
            if  ( newIndex < floorTiles.Length ) {
                return newIndex;
            }
        } else {
            int newIndex = index - columns;
            if ( newIndex >= 0 ) {
                return newIndex;
            }
        }

        return -1;
    }
    public bool CanMoveIndex( int index, Direction dir ) {
        if ( dir == Direction.EAST ) {
            return ( index % columns != columns - 1 );
        } else if ( dir == Direction.WEST ) {
            return index % columns != 0;
        } else if ( dir == Direction.NORTH ) {
            return index + columns < floorTiles.Length;
        } else {
            return index - columns >= 0;
        }
    }

    public bool CanMoveVec2( Vector2 position, int xMove, int yMove ) {
        int index = GetPositionIndex( position.x, position.y );
        Vector2 vector = new Vector2( xMove, yMove );
        Direction dir = GetDirection( vector );
        return CanMoveIndex( index, dir );
    }

    void CalculateRanges( Unit unit ) {
        List<int> frontier = new List<int>();
        List<int> attackFrontier = new List<int>();
        int[] moveCost = new int[ floorTiles.Length ];
        int[] attackMoveCost = new int[ floorTiles.Length ];
        for ( int i = 0; i < moveCost.Length; ++i ) {
            moveCost[ i ] = -1;
            attackMoveCost[ i ] = 0;
        }

        int maxRange = unit.GetCurrentMovementRange();
        int maxAttackRange = unit.GetMaxAttackRange();
        int startIndex = GetPositionIndex( (int)unit.transform.position.x, (int)unit.transform.position.y );
        unitMovementType_t movementType = unit.GetMovementType();
        moveCost[ startIndex ] = 0;
        frontier.Add( startIndex );

        while ( frontier.Count != 0 ) {
           
            int currentIndex = frontier[ 0 ];
            frontier.RemoveAt( 0 );
            int currentRange = moveCost[ currentIndex ];
            if ( currentRange == maxRange ) {
                //Get attack range
                attackFrontier.Add( currentIndex );
                continue;
            }
            Tile currentTile = floorTiles[ currentIndex ].GetComponent<Tile>();
            if ( currentTile != null ) {
                foreach ( Tile nextTile in currentTile.neighbors ) {
                    int newIndex = GetPositionIndex( nextTile.transform.position );
                    if ( moveCost[ newIndex ] < 0 ) {
                        int nextRange = currentRange + nextTile.GetMoveCost( movementType );
                        if ( nextRange > maxRange && !attackFrontier.Contains( currentIndex ) ) {
                            attackFrontier.Add( currentIndex );
                        } else {
                            moveCost[ newIndex ] = nextRange;
                            frontier.Add( newIndex );
                        }
                    }
                }
            }
        }

        while ( attackFrontier.Count != 0 ) {
            int currentIndex = attackFrontier[ 0 ];
            attackFrontier.RemoveAt( 0 );
            int currentAttackRange = attackMoveCost[ currentIndex ];
            if ( currentAttackRange == maxAttackRange ) {
                continue;
            }
            Tile currentTile = floorTiles[ currentIndex ].GetComponent<Tile>();
            if ( currentTile != null ) {
                foreach ( Tile nextTile in currentTile.neighbors ) {
                    int newIndex = GetPositionIndex( nextTile.transform.position );
                    if ( moveCost[ newIndex ] < 0 && attackMoveCost[ newIndex ] <= 0 ) {
                        attackMoveCost[ newIndex ] = currentAttackRange + 1;
                        attackFrontier.Add( newIndex );
                    }
                }
            }
        }

        for ( int i = 0; i < moveCost.Length; ++i ) {
            if ( moveCost[ i ] >= 0 ) {
                unit.movementRangeList.Add( GetPositionFromIndex( i ) );
            }
            if ( attackMoveCost[i] > 0 ) {
                unit.attackRangeList.Add( GetPositionFromIndex( i ) );
            }
        }
    }

    public int CalcDistanceSquared( Vector2 vec1, Vector2 vec2 ) {
        return (int)( Math.Abs( vec1.x - vec2.x ) + Math.Abs( vec1.y - vec2.y ) );
    }

    public int CalcDistanceSquared ( int index1, int index2 ) {
        Vector2 vec1 = GetPositionFromIndex( index1 );
        Vector2 vec2 = GetPositionFromIndex( index2 );
        return CalcDistanceSquared( vec1, vec2 );
    }

    public bool FindShortestPath( Unit unit, Vector2 goalVec, List<int> path ) {
        int goalIndex = GetPositionIndex( goalVec.x, goalVec.y );
        List<int> frontier = new List<int>();
        int[] priorityList = new int[ floorTiles.Length ];
        int[] moveCost = new int[ floorTiles.Length ];
        int[] cameFrom = new int[ floorTiles.Length ];
        int gridSize = rows * columns;
        for ( int i = 0; i < moveCost.Length; ++i ) {
            priorityList[ i ] = gridSize;
            moveCost[ i ] = -1;
            cameFrom[ i ] = -1;
        }
        unitMovementType_t movementType = unit.GetMovementType();
        int maxRange = unit.GetCurrentMovementRange();
        int startIndex = GetPositionIndex( (int)unit.transform.position.x, (int)unit.transform.position.y );
        frontier.Add( startIndex );
        moveCost[ startIndex ] = 0;
        priorityList[ startIndex ] = CalcDistanceSquared( startIndex, goalIndex );

        while ( frontier.Count != 0 ) {

            int currentIndex = frontier[ 0 ];
            frontier.RemoveAt( 0 );
            int currentRange = moveCost[ currentIndex ];
             if ( currentIndex == goalIndex ) {
                path.Add( currentIndex );
                int pathIndex = currentIndex;
                while ( cameFrom[ pathIndex ] >= 0 ) {
                    path.Add( cameFrom[ pathIndex ] );
                    pathIndex = cameFrom[ pathIndex ];
                }
                return true;
            } else if ( currentRange == maxRange ) {
                continue;
            }
            // Movement Ranges
            Tile currentTile = floorTiles[ currentIndex ].GetComponent<Tile>();
            if ( currentTile != null ) {
                foreach ( Tile nextTile in currentTile.neighbors ) {
                    int newIndex = GetPositionIndex( nextTile.transform.position );
                    int newCost = currentRange + nextTile.GetMoveCost( movementType );
                    if ( newCost <= maxRange && ( moveCost[ newIndex ] < 0 || newCost < moveCost[ newIndex ] ) ) {
                        moveCost[ newIndex ] = newCost;
                        int priorityValue = newCost + CalcDistanceSquared( goalIndex, newIndex );
                        bool found = false;
                        for ( int f = 0; f < frontier.Count; ++f ) {
                            if ( priorityValue < priorityList[ frontier[ f ] ] ) {
                                found = true;
                                frontier.Insert( f, newIndex );
                                priorityList[ newIndex ] = priorityValue;
                                cameFrom[ newIndex ] = currentIndex;
                                break;
                            }
                        }
                        if ( !found ) {
                            frontier.Add( newIndex );
                        }
                    }
                }
            }
        }
        return false;
    }

    public void SetupScene( int level ) {
        //Creates the outer walls and floor.
        BoardSetup();
        UnitSetup();
        //Reset our list of gridpositions.
        InitialiseList();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject GetCurrentTile() {
        if ( cursorInstance != null ) {
            int x = (int)cursorInstance.transform.position.x;
            int y = (int)cursorInstance.transform.position.y;
            int tileIndex = x + columns * y;
            if ( tileIndex >= 0 && tileIndex < floorTiles.Length ) {
                return floorTiles[ tileIndex ];
            }
        }
        return null;
    }

    public Tile GetTileByIndex( int tileIndex ) {
        if ( tileIndex >= 0 && tileIndex < floorTiles.Length ) {
            return floorTiles[ tileIndex ].GetComponent<Tile>();
        }
        return null;
    }

    public Unit GetCurrentUnit() {
        GameObject curTile = GetCurrentTile();
        if ( curTile != null ) {
            Tile tile = curTile.GetComponent<Tile>();
            if ( tile != null ) {
                return tile.unit;
            }
        }
        return null;
    }
}
