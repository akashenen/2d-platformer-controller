using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores checkpoints so the player can return to them. Soft checkpoints should be common and serve as 
/// quick return points for when the player is hurt or goes out of bounds. Hard checkpoints should be used as
/// save points and where the player returns if they die
/// </summary>
public class CheckpointSystem : MonoBehaviour {

    public Vector3 softCheckpoint;
    public Vector3 hardCheckpoint;

    /// <summary>
    /// Returns to the last soft checkpoint touched
    /// </summary>
    public void ReturnToSoftCheckpoint() {
        transform.position = softCheckpoint;
    }

    /// <summary>
    /// Returns to the last hard checkpoint touched
    /// </summary>
    public void ReturnToHardCheckpoint() {
        transform.position = softCheckpoint;
    }

}