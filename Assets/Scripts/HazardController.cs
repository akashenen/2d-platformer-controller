using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for environmental hazards that cause harm to characters
/// </summary>
public class HazardController : MonoBehaviour {

    public float damage;
    public float knockbackForce;
    public float stunDuration;
    public float invulnerableDuration;
    public bool airStagger;
    public bool playerOnly;
    public bool softRespawn;
    public bool instantKill;

    private PhysicsConfig pConfig;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        pConfig = GameObject.FindObjectOfType<PhysicsConfig>();
        if (!pConfig) {
            pConfig = (PhysicsConfig) new GameObject().AddComponent(typeof(PhysicsConfig));
            pConfig.gameObject.name = "Physics Config";
            Debug.LogWarning("PhysicsConfig not found on the scene! Using default config.");
        }
    }

    /// <summary>
    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other) {
        if (playerOnly && pConfig.characterMask != LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer))) {
            return;
        }
        CharacterController2D character = other.GetComponent<CharacterController2D>();
        if (character && !character.Invulnerable) {
            if (knockbackForce > 0) {
                Vector2 force = character.TotalSpeed.normalized * -1 * knockbackForce;
                character.Knockback(force, stunDuration);
            }
            if (invulnerableDuration > 0) {
                character.setInvunerable(invulnerableDuration);
            }
            if (airStagger) {
                character.SetAirStagger(stunDuration);
            }
            PlayerController player = other.GetComponent<PlayerController>();
            if (softRespawn && player) {
                player.SoftRespawn();
            }
        }
    }
}