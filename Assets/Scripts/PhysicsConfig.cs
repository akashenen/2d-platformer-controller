using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsConfig : MonoBehaviour {
    public LayerMask groundMask;
    public LayerMask owPlatformMask;
    public LayerMask allPlatformsMask;
    public LayerMask ladderMask;
    public LayerMask playerMask;
    public LayerMask enemyMask;
    public float gravity = -50f;
    public float airFriction = 30f;
    public float groundFriction = 50f;

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
        if (allPlatformsMask == 0) {
            allPlatformsMask = LayerMask.GetMask("Ground", "OWPlatform");
        }
        if (ladderMask == 0) {
            ladderMask = LayerMask.GetMask("Ladder");
        }
        if (playerMask == 0) {
            playerMask = LayerMask.GetMask("Player");
        }
        if (enemyMask == 0) {
            enemyMask = LayerMask.GetMask("Enemy");
        }
    }
}