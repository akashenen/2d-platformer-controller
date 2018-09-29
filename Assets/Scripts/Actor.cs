using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Controller2D))]
/// <summary>
/// Used to store an actor's attributes
/// </summary>
public class Actor : MonoBehaviour {

    // Actor's base stats
    public float moveSpeed;
    public float accelerationTime;
    public float decelerationTime;
    public int maxExtraJumps;
    public int extraJumps;
    public float jumpHeight;
    public float jumpHoldScale;
    public float groundCheckOffset = 0.1f;
    public bool advancedAirControl = false;
    public float airAcceleration;
}