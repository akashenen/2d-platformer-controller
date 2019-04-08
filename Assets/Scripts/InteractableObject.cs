using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class for interactable objects, which should have different interact implementations
/// depending on their function
/// </summary>
public abstract class InteractableObject : MonoBehaviour {
    public bool interactable = true;
    protected InteractSystem system;
    public abstract void Interact(InteractSystem _system);
}