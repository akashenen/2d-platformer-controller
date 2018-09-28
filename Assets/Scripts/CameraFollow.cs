using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the following movement of a camera based on the movement of a Transform
/// /// </summary>
public class CameraFollow : MonoBehaviour {
    // Damp time is a delay for the camera movement (higher time = higher delay)
    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    // Update is called once per frame
    void Update() {
        if (target) {
            Vector3 point = Camera.main.WorldToViewportPoint(target.position);
            Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }
    }
}