using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInspectorWidget : MonoBehaviour {

    public Text tileName;
    public Text unitName;
    private GameObject currentTile;
    private Unit currentUnit;
    private BoardManager boardManager;

    // Use this for initialization
    void Start() {
        if ( boardManager == null ) {
            boardManager = BoardManager.instance;
        }
    }

    // Update is called once per frame
    void Update() {
        if ( boardManager != null ) {
            GameObject tileObj = boardManager.GetCurrentTile();
            if ( currentTile != tileObj ) {
                Unit unit = boardManager.GetCurrentUnit();
                if ( unit != null & unitName != null ) {
                    currentUnit = unit;
                    unitName.text = currentUnit.GetName();
                }
                currentTile = tileObj;
                if ( tileName != null && currentTile != null ) {
                    Tile tile = currentTile.GetComponent<Tile>();
                    if ( tile != null ) {
                        tileName.text = tile.tileName;
                    }
                }
            }
        }
    }
}
