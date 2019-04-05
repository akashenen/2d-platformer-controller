using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController2D))]
/// <summary>
/// Used to store an character's attributes
/// </summary>
public class CharacterData : MonoBehaviour {

    // Character's attibrutes
    [Header("Movement")]
    public float maxSpeed;
    public float accelerationTime;
    public float decelerationTime;
    public bool canUseSlopes;
    [Header("Jumping")]
    public int maxExtraJumps;
    public float maxJumpHeight;
    public float minJumpHeight;
    public bool advancedAirControl;
    public float airAccelerationTime;
    public float airDecelerationTime;
    [Header("Wall Sliding/Jumping")]
    public bool canWallSlide;
    public float wallSlideSpeed;
    public bool canWallJump;
    public float wallJumpSpeed;
    [Header("Dashing")]
    public bool canDash;
    public bool omnidirectionalDash;
    public bool dashDownSlopes;
    public bool canJumpDuringDash;
    public bool jumpCancelStagger;
    public float dashDistance;
    public float dashSpeed;
    public float dashStagger;
    public float maxDashCooldown;
    public int maxAirDashes;
    [Header("Ladders")]
    public float ladderSpeed;
    public float ladderAccelerationTime;
    public float ladderDecelerationTime;
    public float ladderJumpHeight;
    public float ladderJumpSpeed;
}