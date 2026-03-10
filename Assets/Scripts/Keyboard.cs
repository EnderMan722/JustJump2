using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class KingOfThievesController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    private int moveDirection = 1;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Better Jump")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 2f;

    [Header("Wall Jump")]
    public float wallJumpHorizontalForce = 8f;

    [Header("Detection")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private Rigidbody2D rb;
    private Collider2D coll;

    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallHolding;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        HandleTimers();
        HandleInput();
        HandleBetterJump();
    }

    void FixedUpdate()
    {
        CheckSurroundings();
        HandleWallSlide();
        HandleWallHold();
        AutoMove();
    }

    // ===============================
    // AUTO MOVE
    // ===============================
    void AutoMove()
    {
        if (isWallSliding || isWallHolding)
            return;

        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    // ===============================
    // INPUT
    // ===============================
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            jumpBufferCounter = jumpBufferTime;

            // WALL JUMP
            if (isWallSliding || isWallHolding)
            {
                WallJump();
                return;
            }
        }

        // Normal jump (not during wall hold)
        if (jumpBufferCounter > 0 && coyoteCounter > 0 && !isWallHolding)
        {
            Jump();
            jumpBufferCounter = 0;
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void WallJump()
    {
        isWallSliding = false;
        isWallHolding = false;

        // Flip direction
        moveDirection *= -1;
        Flip();

        rb.velocity = Vector2.zero;

        // Apply force away from wall
        Vector2 force = new Vector2(moveDirection * wallJumpHorizontalForce, jumpForce);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    // ===============================
    // TIMERS
    // ===============================
    void HandleTimers()
    {
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    // ===============================
    // BETTER JUMP
    // ===============================
    void HandleBetterJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y *
                           (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 &&
                !Input.GetKey(KeyCode.Space) &&
                !Input.GetMouseButton(0))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y *
                           (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    // ===============================
    // WALL SLIDE
    // ===============================
    void HandleWallSlide()
    {
        isWallSliding = isTouchingWall && !isGrounded && rb.velocity.y < 0;

        if (isWallSliding)
        {
            rb.velocity = new Vector2(0f, -wallSlideSpeed);
        }
    }

    // ===============================
    // WALL HOLD
    // ===============================
    void HandleWallHold()
    {
        if (isGrounded && isTouchingWall && !isWallSliding)
        {
            isWallHolding = true;
            rb.velocity = Vector2.zero;
        }

        if (!isTouchingWall)
        {
            isWallHolding = false;
        }
    }

    // ===============================
    // COLLISION CHECKS
    // ===============================
    void CheckSurroundings()
    {
        isGrounded = Physics2D.Raycast(
            coll.bounds.center,
            Vector2.down,
            coll.bounds.extents.y + 0.02f,
            groundLayer
        );

        isTouchingWall = Physics2D.Raycast(
            coll.bounds.center,
            Vector2.right * moveDirection,
            coll.bounds.extents.x + 0.02f,
            wallLayer
        );
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
