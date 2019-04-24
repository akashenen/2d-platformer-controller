using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsConfig : MonoBehaviour {
    [Tooltip("Which layers are considered ground")]
    public LayerMask groundMask;
    [Tooltip("Which layers are considered one way platforms")]
    public LayerMask owPlatformMask;
    [Tooltip("Which layers are considered ladders")]
    public LayerMask ladderMask;
    [Tooltip("Which layers are considered characters")]
    public LayerMask characterMask;
    [Tooltip("Which layers characters can collide with")]
    public LayerMask characterCollisionMask;
    [Tooltip("Which layers stand-on objects will move")]
    public LayerMask standOnCollisionMask;
    [Tooltip("Which layers are considered interactable objects")]
    public LayerMask interactableMask;
    public float gravity = -30f;
    public float airFriction = 15f;
    public float groundFriction = 30f;
    public float staggerSpeedFalloff = 50f;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        if (groundMask == 0) {
            groundMask = LayerMask.GetMask("Ground");
        }
        if (owPlatformMask == 0) {
            owPlatformMask = LayerMask.GetMask("OWPlatform");
        }
        if (characterCollisionMask == 0) {
            characterCollisionMask = LayerMask.GetMask("Ground");
        }
        if (ladderMask == 0) {
            ladderMask = LayerMask.GetMask("Ladder");
        }
        if (characterMask == 0) {
            characterMask = LayerMask.GetMask("Character");
        }
        if (standOnCollisionMask == 0) {
            standOnCollisionMask = LayerMask.GetMask("Character", "Box");
        }
        if (interactableMask == 0) {
            interactableMask = LayerMask.GetMask("Box");
        }
    }
}