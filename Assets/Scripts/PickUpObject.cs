using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementation of an interactable object that can be picked up and thrown
/// </summary>
[RequireComponent(typeof(ObjectController2D))]
public class PickUpObject : InteractableObject {

    public ObjectController2D Controller { get { return GetComponent<ObjectController2D>(); } }

    private Transform oldParent;

    /// <summary>
    /// When interacted with, the object will attach itself to the character and disable its collisions
    /// </summary>
    /// <param name="_system">The character that interacted with this object</param>
    public override void Interact(InteractSystem _system) {
        system = _system;
        system.PickedUpObject = this;
        transform.position = system.transform.position + (Vector3) system.pickupPositionOffset;
        oldParent = transform.parent;
        transform.parent = system.transform;
        Controller.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    /// <summary>
    /// Releases the object from the character, enables its collisions and applies the specified force
    /// </summary>
    /// <param name="force">Force to be applied</param>
    public void Throw(Vector2 force) {
        Controller.enabled = true;
        GetComponent<Collider2D>().enabled = true;
        CharacterController2D character = system.GetComponent<CharacterController2D>();
        Controller.ApplyForce(new Vector2(force.x * (character.FacingRight ? 1 : -1), force.y) +
            character.TotalSpeed);
        transform.parent = oldParent;
        system.PickedUpObject = null;
    }
}