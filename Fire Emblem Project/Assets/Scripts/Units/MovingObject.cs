﻿using UnityEngine;
using System.Collections;

//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
public abstract class MovingObject : MonoBehaviour {
    public float moveTime = 0.1f;           //Time it will take object to move, in seconds.
    public LayerMask blockingLayer;         //Layer on which collision will be checked.


    private BoxCollider2D boxCollider;      //The BoxCollider2D component attached to this object.
    private Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object.
    private float inverseMoveTime;          //Used to make movement more efficient.
    protected bool isMoving = false;


    //Protected, virtual functions can be overridden by inheriting classes.
    protected virtual void Start() {
        //By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
        inverseMoveTime = 1f / moveTime;

        //Get a component reference to this object's BoxCollider2D
        //boxCollider = GetComponent<BoxCollider2D>();

        //Get a component reference to this object's Rigidbody2D
        rb2D = GetComponent<Rigidbody2D>();
    }


    //Move returns true if it is able to move and false if not. 
    //Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
    protected void Move( int xDir, int yDir ) {
        //Store start position to move from, based on objects current transform position.
        Vector2 start = transform.position;

        // Calculate end position based on the direction parameters passed in when calling Move.
        Vector2 end = start + new Vector2( xDir, yDir );

        //Check if anything was hit
        //if ( hit.transform == null ) {
        //If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
        isMoving = true;
        StartCoroutine( SmoothMovement( end ) );
    }

    protected void MoveTo( Vector2 end ) {
        isMoving = true;
        StartCoroutine( SmoothMovement( end ) );
    }

    protected virtual void FinishedMoving() {
        isMoving = false;
    }

    //Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
    protected IEnumerator SmoothMovement( Vector3 end ) {
        //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
        //Square magnitude is used instead of magnitude because it's computationally cheaper.
        float sqrRemainingDistance = ( transform.position - end ).sqrMagnitude;

        //While that distance is greater than a very small amount (Epsilon, almost zero):
        while ( sqrRemainingDistance > float.Epsilon ) {
            //Find a new position proportionally closer to the end, based on the moveTime
            Vector3 newPostion = Vector3.MoveTowards( rb2D.position, end, inverseMoveTime * Time.deltaTime );

            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            rb2D.MovePosition( newPostion );

            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = ( transform.position - end ).sqrMagnitude;

            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
        }
        FinishedMoving();
    }


    //The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keyword.
    //AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
    protected virtual void AttemptMove( int xDir, int yDir ) {
        //Hit will store whatever our linecast hits when Move is called.
        if ( !isMoving ) {
            Move( xDir, yDir);
        }
    }
}
