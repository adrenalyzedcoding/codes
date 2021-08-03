/*

This script was developed by adrenalyzed.

This script is copyright free, and you don't have to mention me, 
but do NOT say that you have made this and take the credit.

Channel link: https://www.youtube.com/channel/UCQbuBksVuaLcIIfykbhIZtw

*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(BoxCollider2D))]
public class Perfect2DController : MonoBehaviour
{
    [Header("Data")]
    public Perfect2DControllerData data;
    [Space(5f)]
    public Transform cam;
    
    Rigidbody2D rb;
    BoxCollider2D col;

    bool jumping;
    [HideInInspector] public bool canJump;
    bool startBuffer;
    bool facingRight = true;

    float jumpBufferTimer;

    int howManyJumps;

    RaycastHit2D wallL;
    RaycastHit2D wallR;

    Vector2 offset;
    Vector2 size;

    Vector2 vel2Counter;
    Vector2 vel2Slide;

    Vector3 vel3Cam;

    void Start() 
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        offset = col.offset;
        size = col.size;

        jumpBufferTimer = data.jumpBufferTime;
    }

    void Update() 
    {
        JumpInput();
        Flip();

        /// </summary>
        /// Debug.
        /// </summary>

        Debug.DrawRay(col.bounds.center, new Vector2(0f, col.size.y / 2f + data.jumpDetDist) * -1f, Color.red);
        Debug.DrawRay(col.bounds.center, new Vector2(col.size.x / 2f + data.wallDecDist, 0f), Color.red);
        Debug.DrawRay(col.bounds.center, new Vector2(col.size.x / 2f + data.wallDecDist, 0f) * -1f, Color.red);
    }

    void FixedUpdate() 
    {
        Movement();
        Jump();
        CanJump();

        switch (data.jumpMode) 
        {
            case JumpMode.Decided:
            DecidedJump();
            break;

            default:
            break;
        }

        DetectWall();

        if (Input.GetKey(data.slide)) StartCrouch();
        else StopCrouch();

        CameraFollow();
    }

    void JumpInput() 
    {
        /// </summary>
        /// Gives consistent input for jumping.
        /// </summary>

        if (Input.GetKeyDown(data.jump)) 
        {
            startBuffer = true; 
            jumpBufferTimer = 0f;
        }

        if (startBuffer && !canJump) jumpBufferTimer += Time.deltaTime;
        if (startBuffer && jumpBufferTimer <= data.jumpBufferTime && canJump) 
        {
            jumping = true;
            startBuffer = false;
        }
    }

    void Movement() 
    {
        if (Input.GetKey(data.slide)) return;

        /// </summary>
        /// Caps the velocity.
        /// </summary>

        if (rb.velocity.x > data.velTargetX && rb.velocity.x > -data.velTargetX) rb.velocity = new Vector2(data.velTargetX, rb.velocity.y);
        if (rb.velocity.y > data.velTargetY && rb.velocity.y > -data.velTargetY) rb.velocity = new Vector2(rb.velocity.x, data.velTargetY);
        if (rb.velocity.x < -data.velTargetX && rb.velocity.x < data.velTargetX) rb.velocity = new Vector2(-data.velTargetX, rb.velocity.y);
        if (rb.velocity.y < -data.velTargetY && rb.velocity.y < data.velTargetY) rb.velocity = new Vector2(rb.velocity.x, -data.velTargetY);

        /// </summary>
        /// Moves the player character around.
        /// </summary>

        if (!Input.GetKey(data.left) && !Input.GetKey(data.right) || Input.GetKey(data.left) && Input.GetKey(data.right)) 
        {
            CounterMovement();
            return;
        }

        if (Input.GetKey(data.left)) rb.AddForce(Vector2.left * data.acceleration * Time.deltaTime, ForceMode2D.Impulse);
        if (Input.GetKey(data.right)) rb.AddForce(Vector2.right * data.acceleration * Time.deltaTime, ForceMode2D.Impulse);
    }

    void CounterMovement() 
    {
        /// </summary>
        /// Counters the movement for snappier controls.
        /// </summary>

        rb.velocity = Vector2.SmoothDamp(rb.velocity, new Vector2(0f, rb.velocity.y), ref vel2Counter, data.deceleration * Time.deltaTime);
    }

    void Jump() 
    {
        /// </summary>
        /// Lets the player jump.
        /// </summary>

        if (!jumping) return;

        rb.velocity = new Vector2(rb.velocity.x, data.jumpForce * Time.deltaTime);
        howManyJumps --;

        jumping = false;
    }

    void DecidedJump() 
    {
        if (rb.velocity.y < 0f) 
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (data.fallMultiplier - 1f) * Time.deltaTime;
        } else if (rb.velocity.y > 0f && !Input.GetKey(data.jump)) 
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (data.lowJumpMultiplier - 1f) * Time.deltaTime;
        }
    }

    void CanJump() 
    {
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, data.jumpDetDist, data.whatIsGround);

        if (!hit.collider && howManyJumps > 0 || hit.collider) canJump = true;
        if (!hit.collider && howManyJumps <= 0) canJump = false;

        if (hit.collider) howManyJumps = data.howManyJumps - 1;
    }


    void DetectWall() 
    {
        wallL = Physics2D.BoxCast(col.bounds.center, col.size, 0f, Vector2.left, data.wallDecDist, data.whatIsWall);
        wallR = Physics2D.BoxCast(col.bounds.center, col.size, 0f, Vector2.right, data.wallDecDist, data.whatIsWall);

        if (wallL.collider || wallR.collider) StartWallJump();
        else StopWallJump();
    }

    void StartWallJump() 
    {
        rb.AddForce(Vector2.down * data.wallDownForce * Time.deltaTime, ForceMode2D.Force);

        howManyJumps = 0;
        rb.gravityScale = 0f;

        if (wallR.collider && Input.GetKey(data.jump)) 
        {
            rb.AddForce(Vector2.left * data.wallSideForce * 100f * Time.deltaTime, ForceMode2D.Force);
            rb.velocity = new Vector2(rb.velocity.x, data.wallJumpForce * Time.deltaTime);
        }
        if (wallL.collider && Input.GetKey(data.jump)) 
        {
            rb.AddForce(Vector2.right * data.wallSideForce * 100f * Time.deltaTime, ForceMode2D.Force);
            rb.velocity = new Vector2(rb.velocity.x, data.wallJumpForce * Time.deltaTime);
        }
    }

    void StopWallJump() 
    {
        rb.gravityScale = 1f;
    }

    void StartCrouch() 
    {
        col.offset = data.offset;
        col.size = data.size;

        rb.velocity = Vector2.SmoothDamp(rb.velocity, new Vector2(0f, rb.velocity.y), ref vel2Slide, data.slideSmoothing * 100f * Time.deltaTime);
        if (rb.velocity.x < 0f) rb.AddForce(Vector2.left * data.slideForce * Time.deltaTime, ForceMode2D.Force);
        else if (rb.velocity.x > 0f) rb.AddForce(Vector2.right * data.slideForce * Time.deltaTime, ForceMode2D.Force);

        howManyJumps = 0;
    }

    void StopCrouch() 
    {
        col.offset = offset;
        col.size = size;
    }

    void Flip() 
    {
        if (Input.GetKey(data.slide) || Input.GetKey(data.left) && Input.GetKey(data.right) || !data.flipping) return;

        if (Input.GetKey(data.right) && !facingRight || Input.GetKey(data.left) && facingRight) 
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            facingRight = !facingRight;
        }
    }

    void CameraFollow() 
    {
        if (!data.cameraFollow) return;

        cam.position = Vector3.SmoothDamp(cam.position, transform.position + data.camOffset, ref vel3Cam, data.camSmooth * Time.deltaTime);
    }
}