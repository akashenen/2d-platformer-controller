using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger that can be interacted with by a character
/// </summary>
[RequireComponent(typeof(TriggerObject))]
public class InteractableTrigger : InteractableObject {
    private TriggerObject trigger;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        trigger = GetComponent<TriggerObject>();
    }

    /// <summary>
    /// When a character interacts with this object, toggle the trigger
    /// </summary>
    /// <param name="_system"></param>
    public override void Interact(InteractSystem _system) {
        trigger.Trigger();
        // if it's an one shot trigger, disables the interaction
        if (trigger.oneShot) {
            interactable = !trigger.Active;
        }
    }
}