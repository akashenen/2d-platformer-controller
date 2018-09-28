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

    private static readonly float JUMP_HOLD_SCALE = 0.9f;

    // Use this for initialization
    void Start() {
        actor = GetComponent<Actor>();
        controller2D = GetComponent<Controller2D>();
        GameManager.Instance.players.Add(actor);
    }

    // Update is called once per frame
    void Update() {
        controller2D.Walk(Input.GetAxis("Horizontal"));
        if (Input.GetButtonDown("Jump"))
            controller2D.Jump();
        if (Input.GetButton("Jump"))
            controller2D.gravityScale = JUMP_HOLD_SCALE;
        else
            controller2D.gravityScale = 1;
    }
}