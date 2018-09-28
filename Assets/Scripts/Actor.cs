using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Controller2D))]
/// <summary>
/// Represents a character in the game. Controls stats, combat and events.
/// </summary>
public class Actor : MonoBehaviour {

    // Actor's base stats
    [Header("Movement")]
    [Space]
    public float moveSpeed;
    public int maxExtraJumps;
    public int extraJumps;
    public float jumpHeight;
    public float groundCheckOffset = 0.1f;

    public bool FacingRight { get { return controller2D.FacingRight; } }

    // Other Components
    private Controller2D controller2D;
    private Animator animator;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        controller2D = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        controller2D.Move();
        controller2D.HandleKnockback();
    }

}