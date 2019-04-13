using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Area that saves the checkpoint if a character enters it
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ChekpointArea : MonoBehaviour {

    public Transform checkpoint;

    private PhysicsConfig pConfig;

    // Start is called before the first frame update
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
        CheckpointSystem cs = other.GetComponent<CheckpointSystem>();
        if (cs) {
            cs.softCheckpoint = checkpoint.position;
        }
    }
}