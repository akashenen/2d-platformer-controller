using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
/// <summary>
/// Returns the player to it's last checkpoint if it touches this object's collider, 
/// useful when going out of bounds
/// </summary>
public class CheckpointReturnTrigger : MonoBehaviour {

    public bool hardCheckpoint;

    /// <summary>
    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other) {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc) {
            if (hardCheckpoint) { } else {
                pc.SoftRespawn();
            }
        }
    }
}