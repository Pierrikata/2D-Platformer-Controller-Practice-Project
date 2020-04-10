using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizintalInputDirection, verticalInputDirection;
    private bool isFacingRight = true, isRunning, isGrounded, canJump, isTouchingWall, isWallSliding;
    private Rigidbody2D rb;
    private Animator anim;

    // TODO: edit this for acceleration
    public float movementSpeed = 20.0f, jForce = 16.0f, groundCheckRadius, wallCheckDistance,
        wallSlidingSpeed, movementForceInAir, airDragMultiplier = 0.95f, variableJHeightMultiplier = .05f;
    public Transform groundCheck, wallCheck;
    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start()
    {
        // store a reference to the rigidbody on the player
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
    }

    // flip avatar if it faces opposite the direction key
    private void CheckMovementDirection()
    {
        if ((isFacingRight && horizintalInputDirection < 0) || (!isFacingRight && horizintalInputDirection > 0))
            if (!isWallSliding)
            {
                isFacingRight = !isFacingRight;
                transform.Rotate(0.0f, 180.0f, 0.0f);
            }
        if (rb.velocity.x != 0)
            isRunning = true;
        else
            isRunning = false;
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isRunning", isRunning); // set condition to perform running animation
        anim.SetBool("isGrounded", isGrounded); // set condition to perform initial jump animation
        anim.SetFloat("Y_velocity", rb.velocity.y); // // set condition to perform midair jump animation
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        // I omit the third condition where animation is performed if rb.velocity < 0...
        if (isTouchingWall && !isGrounded)
            isWallSliding = true;
        else
            isWallSliding = false;
        // therefore, I'm basically making the character grab the wall
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y <= 0)
            canJump = true;
        else
            canJump = false;
    }

    private void CheckInput()
    {
        // check if player presses WALK button
        horizintalInputDirection = Input.GetAxisRaw("Horizontal");

        // check if player grabs wall
        verticalInputDirection = Input.GetAxisRaw("Vertical");
        if (isTouchingWall && !isGrounded && Input.GetAxisRaw("Horizontal") != 0 && rb.velocity.y <= 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        // check if player presses JUMP button
        if (Input.GetButtonDown("Jump"))
            if (canJump)
                rb.velocity = new Vector2(rb.velocity.x, jForce);

        if (Input.GetButtonUp("Jump"))
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJHeightMultiplier);
    }

    private void ApplyMovement()
    {
        // TODO: edit for deceleration effect on ground
        /*Vector3 currPos = transform.position;
        float deltaX = (horizintalInputDirection * movementSpeed * Time.deltaTime);
        transform.position = new Vector3(currPos.x + deltaX, currPos.y, currPos.z);
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("JUMP!");
            if (rb != null)
                rb.AddForce(new Vector2(0, jForce), ForceMode2D.Impulse);
        }*/

        // core implementation
        if (isGrounded || rb.velocity.x == 0)
            rb.velocity = new Vector2(movementSpeed * horizintalInputDirection, rb.velocity.y);
        else if (!isGrounded && !isWallSliding && horizintalInputDirection != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * horizintalInputDirection, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > movementSpeed)
                rb.velocity = new Vector2(movementSpeed * horizintalInputDirection, rb.velocity.y);
        }
        else if (!isGrounded && !isWallSliding && horizintalInputDirection == 0)
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);

        if (isWallSliding)
            if (rb.velocity.y < -wallSlidingSpeed)
                rb.velocity = new Vector2(movementSpeed * horizintalInputDirection, -wallSlidingSpeed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
