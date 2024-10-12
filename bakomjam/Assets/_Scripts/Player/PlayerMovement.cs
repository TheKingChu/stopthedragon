using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed stuff")]
    public float moveSpeed = 5f;
    public float speedBoostMultiplier = 2f;
    public float speedBoostDuration = 3f;
    private float originalMoveSpeed;
    private bool isSpeedBoosted = false;

    [Header("Screen bounds")]
    public float screenBoundsOffset = 0.5f;

    [Header("Stun")]
    public float stunDuration = 2f;
    private float stunTimer;
    private bool isStunned = false;

    private float screenWidthInWorldUnits;
    private Rigidbody2D rb2d;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        originalMoveSpeed = moveSpeed; //saving the original speed

        //calculate the screen width in world units
        screenWidthInWorldUnits = Camera.main.aspect * Camera.main.orthographicSize * 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStunned)
        {
            StunUpdate();
        }
        else
        {
            MovePlayer();
            ClampPlayerPosition();
        }
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

    private void StunUpdate()
    {
        stunTimer -= Time.deltaTime;
        if(stunTimer <= 0)
        {
            isStunned = false;
        }
    }

    public void StunPlayer()
    {
        isStunned = true;
        stunTimer = stunDuration;
        rb2d.velocity = Vector2.zero;
    }

    public void ApplySpeedBoost()
    {
        if (!isSpeedBoosted)
        {
            StartCoroutine(SpeedBoostCoroutine());
        }
    }

    private IEnumerator SpeedBoostCoroutine()
    {
        isSpeedBoosted = true;
        moveSpeed *= speedBoostMultiplier;
        yield return new WaitForSeconds(speedBoostDuration);
        moveSpeed = originalMoveSpeed;
        isSpeedBoosted = false;
    }
}
