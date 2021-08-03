/*

This script was developed by adrenalyzed.

This script is copyright free, and you don't have to mention me, 
but do NOT say that you have made this and take the credit.

Channel link: https://www.youtube.com/channel/UCQbuBksVuaLcIIfykbhIZtw

*/

using UnityEngine;

public enum JumpMode 
{
    Static,
    Decided
}

[CreateAssetMenu(fileName = "ControllerData", menuName = "Perfect2DController/ControllerData")]
public class Perfect2DControllerData : ScriptableObject
{
    [Header("Keybinds")]
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    [Space(5f)]
    public KeyCode jump = KeyCode.W;
    [Space(5f)]
    public KeyCode slide = KeyCode.S;

    [Header("Movement")]
    public float velTargetX = 10f;
    public float velTargetY = 1000f;
    [Space(5f)]
    public float acceleration = 175f;
    public float deceleration = 6f;

    [Header("Jumping")]
    public JumpMode jumpMode = JumpMode.Decided;
    [Space(5f)]
    public float jumpForce = 700f;
    public int howManyJumps = 2;
    [Space(5f)]
    public float jumpBufferTime = 0.1f;
    [Space(5f)]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    [Space(5f)]
    public float jumpDetDist = 0.05f;
    public LayerMask whatIsGround;

    [Header("Sliding")]
    public Vector2 offset = new Vector2(0f, 0f);
    public Vector2 size = new Vector2(1f, 0.5f);
    [Space(5f)]
    public float slideSmoothing = 5f;
    public float slideForce = 550f;

    [Header("Wall Jump")]
    public float wallSideForce = 1000f;
    public float wallJumpForce = 850f;
    public float wallDownForce = 4500f;
    [Space(5f)]
    public float wallDecDist = 0.05f;
    public LayerMask whatIsWall;

    [Header("Flipping")]
    public bool flipping = true;

    [Header("Camera Follow")]
    public bool cameraFollow = true;
    [Space(5f)]
    public Vector3 camOffset = new Vector3(0f, 3f, -10f);
    [Space(5f)]
    public float camSmooth = 10f;
}