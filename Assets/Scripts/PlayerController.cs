using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(CheckpointSystem))]
/// <summary>
/// Deals with inputs for player characters
/// </summary>
public class PlayerController : MonoBehaviour {

    public float softRespawnDelay = 0.5f;
    public float softRespawnDuration = 0.5f;

    // Other components
    private Controller2D controller2D;
    private CameraController cameraController;
    private CheckpointSystem checkpoint;

    // Use this for initialization
    void Start() {
        controller2D = GetComponent<Controller2D>();
        checkpoint = GetComponent<CheckpointSystem>();
        cameraController = GameObject.FindObjectOfType<CameraController>();
        if (!cameraController) {
            Debug.LogError("The scene is missing a camera controller! The player script needs it to work properly!");
        }
    }

    // Update is called once per frame
    void Update() {
        Vector2 axis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        controller2D.Walk(axis.x);
        controller2D.ClimbLadder(axis.y);
        if (Input.GetButtonDown("Jump")) {
            if (axis.y < 0) {
                controller2D.JumpDown();
            } else {
                controller2D.Jump();
            }
        }
        if (Input.GetButtonUp("Jump"))
            controller2D.EndJump();
        else
            controller2D.SetGravityScale(1);
        if (Input.GetButtonDown("Dash")) {
            controller2D.Dash(axis);
        }
    }

    /// <summary>
    /// Respawns the player at the last soft checkpoint while keeping their current stats
    /// </summary>
    public void SoftRespawn() {
        controller2D.Immobile = true;
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
        controller2D.Immobile = false;
    }
}