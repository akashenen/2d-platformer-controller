using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that enables characters to interact with interactable objects, including picking up if appliable
/// </summary>
[RequireComponent(typeof(CharacterController2D))]
public class InteractSystem : MonoBehaviour {

    [Tooltip("How distant objects can be to be interacted with (visible in a magenta gizmo)")]
    public float interactRange = 0.5f;
    [Tooltip("Offset for the range's starting position relative to the character position")]
    public Vector2 rangePositionOffset;
    [Tooltip("GameObject for the icon that will appear above interactables")]
    public SpriteRenderer inputIcon;
    [Tooltip("Offset for the icon's position relative to the attached object's position")]
    public Vector2 iconPositionOffset;
    [Tooltip("Icon shown when using a keyboad")]
    public Sprite keyboardIcon;
    [Tooltip("Icon shown when using a Xbox-like gamepad")]
    public Sprite xboxIcon;
    [Tooltip("Icon shown when using a PSX-like gamepad")]
    public Sprite psxIcon;
    [Tooltip("When enabled, allows the character to detect and pick up pick-upable objects")]
    public bool canPickup;
    [Tooltip("Offset for the object's position when picked up by a charater")]
    public Vector2 pickupPositionOffset;
    [Tooltip("Amount and direction of the force applied when throwing the picked object")]
    public Vector2 throwForce;

    public PickUpObject PickedUpObject { get; set; }

    private InteractableObject closestObject = null;
    private PhysicsConfig pConfig;
    private CharacterController2D character;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        inputIcon.enabled = false;
        character = GetComponent<CharacterController2D>();
        pConfig = GameObject.FindObjectOfType<PhysicsConfig>();
        if (!pConfig) {
            pConfig = (PhysicsConfig) new GameObject().AddComponent(typeof(PhysicsConfig));
            pConfig.gameObject.name = "Physics Config";
            Debug.LogWarning("PhysicsConfig not found on the scene! Using default config.");
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update() {
        // Tries to find interactable objects within range and sets up the closest one for interaction
        closestObject = null;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + (Vector3) rangePositionOffset,
            interactRange, pConfig.interactableMask);
        foreach (Collider2D hit in hits) {
            InteractableObject obj = hit.GetComponent<InteractableObject>();
            if (obj && obj.interactable && (canPickup && !PickedUpObject || !obj.GetComponent<PickUpObject>()) &&
                (!closestObject || Vector2.Distance(transform.position, closestObject.transform.position) >
                    Vector2.Distance(transform.position, obj.transform.position))) {
                closestObject = obj;
            }
        }
        inputIcon.enabled = closestObject;
        if (closestObject) {
            inputIcon.transform.parent = closestObject.transform;
            inputIcon.transform.position = closestObject.transform.position + (Vector3) iconPositionOffset;
        } else {
            inputIcon.transform.parent = transform;
        }
    }

    /// <summary>
    /// If there's an interactable object within the range, interacts with the closest one
    /// </summary>
    public void Interact() {
        if (closestObject) {
            closestObject.Interact(this);
            closestObject = null;
        }
    }

    /// <summary>
    /// If there's a picked up object, throws it
    /// </summary>
    public void Throw() {
        if (PickedUpObject) {
            PickedUpObject.Throw(throwForce);
        }
    }

    /// <summary>
    /// Callback to draw gizmos only if the object is selected.
    /// </summary>
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta * new Color(1, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position + (Vector3) rangePositionOffset, interactRange);
    }
}