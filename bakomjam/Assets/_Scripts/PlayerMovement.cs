using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float screenBoundsOffset = 0.5f;

    private float screenWidthInWorldUnits;
    private Rigidbody2D rb2d;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        //calculate the screen width in world units
        screenWidthInWorldUnits = Camera.main.aspect * Camera.main.orthographicSize * 2f;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        ClampPlayerPosition();
    }

    private void MovePlayer()
    {
        float moveInput = Input.GetAxis("Horizontal");
        Vector2 movement = new Vector2(moveInput * moveSpeed, rb2d.velocity.y);
        rb2d.velocity = movement;

        animator.SetFloat("MoveDirection", moveInput);
    }

    private void ClampPlayerPosition()
    {
        //get current position of player
        Vector3 playerPosition = transform.position;

        //calculate the horizontal boundries of the screen
        float screenLeftEdge = -(screenWidthInWorldUnits / 2) + screenBoundsOffset;
        float screenRigthEdge = (screenWidthInWorldUnits / 2) - screenBoundsOffset;

        //clamp the player's x position between left and right edges
        playerPosition.x = Mathf.Clamp(playerPosition.x, screenLeftEdge, screenRigthEdge);


        //apply the clamped position back to the player's transform
        transform.position = playerPosition;
    }
}
