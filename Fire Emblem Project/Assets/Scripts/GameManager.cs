using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.
    private int level = 3;                                  //Current level number, expressed in game as "Day 1".
    [HideInInspector]
    public bool playersTurn = true;
    GameObject actionMenu;
    

    //Awake is always called before any Start functions
    void Awake() {
        //Check if instance already exists
        if ( instance == null )

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if ( instance != this )

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy( gameObject );

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad( gameObject );

        //Get a component reference to the attached BoardManager script
        boardScript = FindObjectOfType<BoardManager>();// GetComponent<BoardManager>();

        //Call the InitGame function to initialize the first level 
        InitGame();
    }

    //Initializes the game for each level.
    void InitGame() {
        //Call the SetupScene function of the BoardManager script, pass it current level number.
        boardScript.SetupScene( level );

    }

    public GameObject GetCurrentTile() {
        return boardScript.GetCurrentTile();
    }

    public void SetBoardManager( BoardManager bm ) {
        boardScript = bm;
    }

    private void Start() {
        actionMenu = GameObject.Find( "Panel" );
    }
    // Update is called once per frame
    void Update () {
		
	}

    public void ShowActionMenu() {
        actionMenu.SetActive( true );
    }

    public void HideActionMenu() {
        actionMenu.SetActive( false );
    }

    public void EndTurnCheck() {

    }
}
