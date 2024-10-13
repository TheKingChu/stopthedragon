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

    private bool isStunned = false;

    private float screenWidthInWorldUnits;
    private Rigidbody2D rb2d;
    public Animator animator;

    private AudioSource audioSource;
    public AudioClip hitSfx;

    [Header("Trail effect")]
    public TrailRenderer speedTrail;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        originalMoveSpeed = moveSpeed; //saving the original speed

        //calculate the screen width in world units
        screenWidthInWorldUnits = Camera.main.aspect * Camera.main.orthographicSize * 2f;

        if(speedTrail != null)
        {
            speedTrail.emitting = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!isStunned)
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

    public void OnHitByStalactite()
    {
        if (!isStunned)
        {
            //step 1 diable movement and fade out sprite
            rb2d.velocity = Vector2.zero;
            isStunned = true;
            animator.SetFloat("MoveDirection", 0);
            animator.ResetTrigger("Reappear");
            animator.SetTrigger("Disappear");
            audioSource.PlayOneShot(hitSfx);

            StartCoroutine(MovePlayerAfterDelay(1f));
        }
    }

    private IEnumerator MovePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        transform.position = new Vector3(0f, -3f, 0f);

        StartCoroutine(ReappearAfterDelay(0.2f));
    }

    private IEnumerator ReappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        animator.ResetTrigger("Disappear");
        animator.SetTrigger("Reappear");
        isStunned = false;
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

        if(speedTrail != null)
        {
            speedTrail.emitting = true;
        }

        moveSpeed *= speedBoostMultiplier;
        yield return new WaitForSeconds(speedBoostDuration);

        moveSpeed = originalMoveSpeed;
        isSpeedBoosted = false;

        if(speedTrail != null)
        {
            speedTrail.emitting = false;
        }
    }
}
