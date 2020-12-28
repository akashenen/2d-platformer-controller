using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(CheckpointSystem))]
/// <summary>
/// Deals with inputs for player characters
/// </summary>
public class PlayerController : MonoBehaviour {

    public float softRespawnDelay = 0.5f;
    public float softRespawnDuration = 0.5f;
    public InputMaster controls;

    // Other components
    private CharacterController2D character;
    private CameraController cameraController;
    private CheckpointSystem checkpoint;
    private InteractSystem interact;
    private Vector2 axis;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake() {
        character = GetComponent<CharacterController2D>();
        checkpoint = GetComponent<CheckpointSystem>();
        interact = GetComponent<InteractSystem>();
        cameraController = GameObject.FindObjectOfType<CameraController>();
        if (!cameraController) {
            Debug.LogError("The scene is missing a camera controller! The player script needs it to work properly!");
        }
        controls = new InputMaster();
        controls.Player.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.Player.Movement.canceled += ctx => Move(Vector2.zero);
        controls.Player.Jump.started += Jump;
        controls.Player.Jump.canceled += EndJump;
        controls.Player.Dash.started += Dash;
        controls.Player.Interact.started += Interact;
        controls.Player.AttackA.started += Attack;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate() {
        character.Walk(axis.x);
        character.ClimbLadder(axis.y);
    }

    private void Move(Vector2 _axis) {
        axis = _axis;
    }

    private void Jump(InputAction.CallbackContext context) {
        if (axis.y < 0) {
            character.JumpDown();
        } else {
            character.Jump();
        }
    }

    private void EndJump(InputAction.CallbackContext context) {
        character.EndJump();
    }

    private void Dash(InputAction.CallbackContext context) {
        character.Dash(axis);
    }

    private void Interact(InputAction.CallbackContext context) {
        if (interact) {
            interact.Interact();
        }
    }

    private void Attack(InputAction.CallbackContext context) {
        if (interact && interact.PickedUpObject) {
            interact.Throw();
        }
    }

    /// <summary>
    /// Respawns the player at the last soft checkpoint while keeping their current stats
    /// </summary>
    public void SoftRespawn() {
        character.Immobile = true;
        Invoke("StartSoftRespawn", softRespawnDelay);
    }

    /// <summary>
    /// Starts the soft respwan after a delay and fades out the screen
    /// </summary>
    private void StartSoftRespawn() {
        cameraController.FadeOut();
        Invoke("EndSoftRespawn", softRespawnDuration);
    }

    /// <summary>
    /// Ends the soft respwan after the duration ended, repositions the player and fades in the screen
    /// </summary>
    private void EndSoftRespawn() {
        checkpoint.ReturnToSoftCheckpoint();
        cameraController.FadeIn();
        character.Immobile = false;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable() {
        controls.Player.Enable();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable() {
        controls.Player.Disable();
    }
}