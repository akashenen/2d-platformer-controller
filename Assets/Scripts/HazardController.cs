using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for environmental hazards that cause harm to actors
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
        if (playerOnly && pConfig.playerMask != LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer))) {
            return;
        }
        Controller2D actor = other.GetComponent<Controller2D>();
        if (actor && !actor.Invulnerable) {
            if (knockbackForce > 0) {
                Vector2 force = actor.TotalSpeed.normalized * -1 * knockbackForce;
                actor.Knockback(force, stunDuration);
            }
            if (invulnerableDuration > 0) {
                actor.setInvunerable(invulnerableDuration);
            }
            if (airStagger) {
                actor.SetAirStagger(stunDuration);
            }
            PlayerController player = other.GetComponent<PlayerController>();
            if (softRespawn && player) {
                player.SoftRespawn();
            }
        }
    }
}