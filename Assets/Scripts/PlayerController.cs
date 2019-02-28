using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(Controller2D))]
/// <summary>
/// Deals with inputs for player characters
/// </summary>
public class PlayerController : MonoBehaviour {
    // Other components
    private Actor actor;
    private Controller2D controller2D;

    // Use this for initialization
    void Start() {
        actor = GetComponent<Actor>();
        controller2D = GetComponent<Controller2D>();
    }

    // Update is called once per frame
    void Update() {
        Vector2 axis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        controller2D.Walk(axis.x);
        if (Input.GetButtonDown("Jump")) {
            if (axis.y < 0) {
                controller2D.JumpDown();
            } else {
                controller2D.Jump();
            }
        }
        if (Input.GetButton("Jump"))
            controller2D.SetGravityScale(actor.jumpHoldScale);
        else
            controller2D.SetGravityScale(1);
        if (Input.GetButtonDown("Dash")) {
            controller2D.Dash(axis);
        }
    }
}