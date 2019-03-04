using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformWaypoint : MonoBehaviour {

    public PlatformWaypoint nextWaipoint;

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmosSelected() {
        if (nextWaipoint) {
            Gizmos.color = Color.cyan * new Color(1, 1, 1, 0.5f);
            Gizmos.DrawLine(transform.position, nextWaipoint.transform.position);
        }
        Gizmos.color = Color.blue * new Color(1, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}