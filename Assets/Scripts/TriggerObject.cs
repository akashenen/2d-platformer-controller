using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic object that can be active or inactive and affect triggerable objects
/// </summary>
public class TriggerObject : MonoBehaviour {

    public static readonly string ANIMATION_ACTIVE = "active";

    [Tooltip("If enabled, the trigger will stay active after triggered")]
    public bool oneShot;

    protected Animator animator;

    /// <summary>
    /// Whether or not the trigger is active
    /// </summary>
    public virtual bool Active { get; protected set; }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Toggles the trigger between active and inactive
    /// </summary>
    public virtual void Trigger() {
        if (!oneShot || !Active) {
            Active = !Active;
            if (animator) {
                animator.SetBool(ANIMATION_ACTIVE, Active);
            }
        }
    }
}