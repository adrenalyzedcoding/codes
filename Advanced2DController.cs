using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Advanced2DController : MonoBehaviour
{

    [Header("MOVEMENT SETTINGS (W, A, S, D)"), Space(2.5f)]
    public float movementForce = 500f;
    [Space(5)]
    public float jumpForce = 10f;
    public float counterJumpForce = 8f;
    [Space(2.5f)]
    [Range(0, 10)] public int howManyJumps = 2;
    [Space(5)]
    public JumpSettings jumpSettings = JumpSettings.Decided;
    public LayerMask whatIsGround;

    [Header("CROUCHING / SLIDING SETTINGS"), Space(2.5f)]

    public Vector2 colliderOffset = new Vector2(0, 0);
    public Vector2 colliderSize = new Vector2(1, 0.5f);
    [Space(2.5f)]
    public float crouchingSpeed = 250f;
    public float slidingSpeed = 2f;
    [Space(5)]
    public CrouchOrSlide crouchOrSlide = CrouchOrSlide.Sliding;

    [Header("CAMERA FOLLOW"), Space(2.5f)]
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    [Range(0, 1)] public float followInterpolation = 0.1f;
    [Space(5)]
    public CameraFollowSettings cameraSettings = CameraFollowSettings.Follow;
    [HideInInspector] public bool useMainCamera = false;
    [HideInInspector] public Camera cam;

    private Rigidbody2D rb;
    private BoxCollider2D bc;

    private bool facingRight = true;
    private bool isCrouching = false, isSliding = false;

    private float movDir;
    private float movementForceR;

    private Vector2 colliderOffsetR, colliderSizeR;

    private int howManyJumpsR;

    public enum CrouchOrSlide {Sliding, Crouching}
    public enum JumpSettings {Static, Decided}
    public enum CameraFollowSettings {Static, Follow}

    private void Awake()
    {
        Assigner();
        Rememberer();
    }

    private void Assigner() 
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
    }

    private void Rememberer() 
    {
        howManyJumps --;
        howManyJumpsR = howManyJumps;
        movementForceR = movementForce;

        colliderOffsetR = bc.offset;
        colliderSizeR = bc.size;
    }

    private void Update()
    {
        Flip();

        if (JumpButton() && CanJump() && !isCrouching && !isSliding) {Jump();}
        switch(jumpSettings) 
        {
            case JumpSettings.Decided:
            if (CounterJumpButton()) {CounterJump();}
            break;
        }

        switch(crouchOrSlide) 
        {
            case CrouchOrSlide.Crouching:
            if (DownButton()) {StartCrouch();}
            if (CounterDownButton()) {StopCrouch();}
            break;

            case CrouchOrSlide.Sliding:
            if (DownButtonContinuous()) {StartSlide();}
            if (CounterDownButton()) {StopSlide();}
            break;
        }

    // to update

        movDir = Input.GetAxis("Horizontal");
        if (IsGrounded()) {howManyJumps = howManyJumpsR;}
    }

    private void FixedUpdate() 
    {
        if (!isSliding) {Movement();}

        switch(cameraSettings) 
        {
            case CameraFollowSettings.Follow:
            CameraFollow();
            break;
        }
    }

    // movement

    private void Movement() 
    {
        rb.velocity = new Vector2(movDir * movementForce * Time.deltaTime, rb.velocity.y);
    }

    // jumping

    private void Jump() 
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        howManyJumps --;
    }

    private void CounterJump() 
    {
        rb.velocity = new Vector2(rb.velocity.x, -counterJumpForce);
    }

    private bool CanJump() {return howManyJumps > 0;}  
    private bool IsGrounded() {RaycastHit2D hit = Physics2D.Raycast(bc.bounds.center, Vector2.down, bc.bounds.extents.y + 0.05f, whatIsGround); return hit.collider != null;}
    private bool JumpButton() {return Input.GetKeyDown(KeyCode.Space) | Input.GetKeyDown(KeyCode.W) | Input.GetKeyDown(KeyCode.UpArrow);}   
    private bool CounterJumpButton() {return Input.GetKeyUp(KeyCode.Space) | Input.GetKeyUp(KeyCode.W) | Input.GetKeyUp(KeyCode.UpArrow);}   
    
    // flipping

    private void Flip() 
    {
        if (movDir > 0 && !facingRight || movDir < 0 && facingRight) 
        {
            facingRight = !facingRight;

            Vector3 flip = transform.localScale;
            flip.x = -flip.x;
            transform.localScale = flip;
        }
    }

    // crouching and sliding

    private void StartCrouch() 
    {
        bc.offset = colliderOffset;
        bc.size = colliderSize;
        isCrouching = true;
        movementForce = crouchingSpeed;
    }

    private void StopCrouch() 
    {
        bc.offset = colliderOffsetR;
        bc.size = colliderSizeR;
        isCrouching = false;
        movementForce = movementForceR;
    }

    private void StartSlide() 
    {
        bc.offset = colliderOffset;
        bc.size = colliderSize;
        isSliding = true;

        if (rb.velocity.x > 0.01f) 
        {
            Vector2 delta = new Vector2(Time.deltaTime, 0);
            rb.velocity -= (delta * slidingSpeed);
        }
    }

    private void StopSlide() 
    {
        bc.offset = colliderOffsetR;
        bc.size = colliderSizeR;
        isSliding = false;
    }

    private bool DownButton() {return Input.GetKeyDown(KeyCode.S) | Input.GetKeyDown(KeyCode.DownArrow);}
    private bool DownButtonContinuous() {return Input.GetKey(KeyCode.S) | Input.GetKey(KeyCode.DownArrow);}
    private bool CounterDownButton() {return Input.GetKeyUp(KeyCode.S) | Input.GetKeyDown(KeyCode.DownArrow);}

    private void CameraFollow() 
    {
        if (!useMainCamera) 
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, transform.position + offset, followInterpolation);
        }
        else 
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position + offset, followInterpolation);
        }
    }
#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(Advanced2DController))]
public class InspectorEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        
        Advanced2DController controller = (Advanced2DController) target;
        GUILayout.Label("This code was made by adrenalyzed.");

        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();

        controller.useMainCamera = EditorGUILayout.Toggle("Use Main Camera", controller.useMainCamera);
        if (!controller.useMainCamera) {controller.cam = EditorGUILayout.ObjectField("", controller.cam, typeof(Camera), true) as Camera;}
        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Press for a fortune!")) 
        {
            Debug.Log("You should subscribe to adrenalyzed and adrenalyzedcantcodeproperly for good luck!");
        }
    }
}
#endif
#endregion
}