using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour {

    public PlatformWaypoint currentWaypoint;
    public float maxSpeed;
    public float accelerationDistance;
    public float decelerationDistance;
    public float waitTime;

    [SerializeField]
    private Vector2 speed = Vector2.zero;
    private float currentWaitTime = 0;
    private List<Controller2D> actors = new List<Controller2D>();

    // Update is called once per frame
    void Update() {
        if (currentWaypoint) {
            if (currentWaitTime > 0) {
                currentWaitTime -= Time.deltaTime;
                return;
            }
            Vector2 distance = currentWaypoint.transform.position - transform.position;
            if (distance.magnitude <= decelerationDistance) {
                if (distance.magnitude > 0) {
                    speed -= Time.deltaTime * distance.normalized * maxSpeed * maxSpeed / (2 * decelerationDistance);
                } else {
                    speed = Vector2.zero;
                }
            } else if (speed.magnitude < maxSpeed) {
                if (accelerationDistance > 0) {
                    speed += Time.deltaTime * distance.normalized * maxSpeed * maxSpeed / (2 * accelerationDistance);
                }
                if (speed.magnitude > maxSpeed || accelerationDistance <= 0) {
                    speed = distance.normalized * maxSpeed;
                }
            }
            Vector3 newPos = Vector2.MoveTowards(transform.position, currentWaypoint.transform.position, speed.magnitude * Time.deltaTime);
            Vector2 velocity = newPos - transform.position;
            if (speed.y > 0) {
                MoveActors(velocity);
                transform.position = newPos;
            } else {
                transform.position = newPos;
                MoveActors(velocity);
            }
            distance = currentWaypoint.transform.position - transform.position;
            if (distance.magnitude < 0.00001f) {
                speed = Vector2.zero;
                currentWaypoint = currentWaypoint.nextWaipoint;
                currentWaitTime = waitTime;
            }
        }
    }

    private void MoveActors(Vector2 velocity) {
        foreach (Controller2D actor in actors) {
            actor.Move(velocity);
        }
    }

    /// <summary>
    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other) {
        Controller2D actor = other.GetComponent<Controller2D>();
        if (actor && !actors.Contains(actor)) {
            // doesn't attach to the actor if it's a 1 way platform and the actor is below it
            if (gameObject.layer == LayerMask.NameToLayer(Controller2D.OW_PLATFORM_LAYER) &&
                actor.transform.position.y < transform.position.y) {
                return;
            } else {
                actors.Add(actor);
            }
        }
    }

    /// <summary>
    /// Sent when another object leaves a trigger collider attached to
    /// this object (2D physics only).
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerExit2D(Collider2D other) {
        Controller2D actor = other.GetComponent<Controller2D>();
        if (actor && actors.Contains(actor)) {
            actors.Remove(actor);
            actor.ApplyForce(speed);
        }
    }
}