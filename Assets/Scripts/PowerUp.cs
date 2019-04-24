using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PowerUp : MonoBehaviour {

    public static readonly string ANIMATION_PICKUP = "pickup";

    public PowerUpType type;
    public float value;
    public bool playerOnly = true;

    private Animator animator;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other) {
        if (playerOnly && !other.GetComponent<PlayerController>()) {
            return;
        }
        CharacterData character = other.GetComponent<CharacterData>();
        if (character) {
            switch (type) {
                case PowerUpType.MoveSpeed:
                    character.maxSpeed += value;
                    break;
                case PowerUpType.ExtraJump:
                    character.maxExtraJumps = Mathf.RoundToInt(value);
                    break;
                case PowerUpType.Dash:
                    character.canDash = true;
                    break;
                case PowerUpType.AirDash:
                    character.canDash = true;
                    character.maxAirDashes += Mathf.RoundToInt(value);
                    break;
                case PowerUpType.WallJump:
                    character.canWallSlide = true;
                    character.canWallJump = true;
                    break;
                default:
                    break;
            }
        }
        animator.SetTrigger(ANIMATION_PICKUP);
    }

    public enum PowerUpType {
        MoveSpeed,
        ExtraJump,
        Dash,
        AirDash,
        WallJump
    }
}