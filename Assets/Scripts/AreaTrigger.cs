using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AreaTrigger : TriggerObject {

    [Tooltip("Which layers' objects can trigger this")]
    public LayerMask triggerMask;
    [Tooltip("If enabled, will only trigger if a player enters the area")]
    public bool playerOnly;

    /// <summary>
    /// Returns true if there's a valid object inside the area
    /// </summary>
    public override bool Active { get { return objCount > 0; } }

    private int objCount;

    /// <summary>
    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other) {
        if (oneShot && objCount > 0) {
            return;
        }
        if (triggerMask == (triggerMask | (1 << other.gameObject.layer)) &&
            (!playerOnly || other.tag == "Player")) {
            objCount++;
            animator.SetBool(ANIMATION_ACTIVE, Active);
        }
    }

    /// <summary>
    /// Sent when another object leaves a trigger collider attached to
    /// this object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerExit2D(Collider2D other) {
        if (oneShot && objCount > 0) {
            return;
        }
        if (triggerMask == (triggerMask | (1 << other.gameObject.layer)) &&
            (!playerOnly || other.tag == "Player")) {
            objCount--;
            animator.SetBool(ANIMATION_ACTIVE, Active);
        }
    }
}