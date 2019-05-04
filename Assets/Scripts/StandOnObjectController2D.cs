using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objects that can support other objects on top of them and will move them when moved
/// </summary>
public class StandOnObjectController2D : ObjectController2D {
    // Start is called before the first frame update
    public override void Start() {
        base.Start();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    public override void FixedUpdate() {
        base.FixedUpdate();
    }

    /// <summary>
    /// Tries to move according to current speed and checking for collisions
    /// </summary>
    public override Vector2 Move(Vector2 deltaMove) {
        int layer = gameObject.layer;
        gameObject.layer = Physics2D.IgnoreRaycastLayer;
        PreMove(ref deltaMove);
        float xDir = Mathf.Sign(deltaMove.x);
        if (deltaMove.x != 0) {
            // Slope checks and processing
            if (deltaMove.y <= 0) {
                if (collisions.onSlope) {
                    if (collisions.groundDirection == xDir) {
                        DescendSlope(ref deltaMove);
                    } else {
                        ClimbSlope(ref deltaMove);
                    }
                }
            }
            HorizontalCollisions(ref deltaMove);
        }
        if (collisions.onSlope && collisions.groundAngle >= minWallAngle && collisions.groundDirection != xDir &&
            speed.y < 0) {
            speed.x = 0;
            Vector2 origin = collisions.groundDirection == -1 ? raycastOrigins.bottomRight : raycastOrigins.bottomRight;
            collisions.hHit = Physics2D.Raycast(origin, Vector2.left * collisions.groundDirection,
                1f, collisionMask);
        }
        bool movedObjs = false;
        if (deltaMove.y > 0) {
            MoveObjectsAbove(deltaMove);
            movedObjs = true;
        }
        if (deltaMove.y > 0 || (deltaMove.y < 0 && (!collisions.onSlope || deltaMove.x == 0))) {
            VerticalCollisions(ref deltaMove);
        }
        if (collisions.onGround && deltaMove.x != 0 && speed.y <= 0) {
            HandleSlopeChange(ref deltaMove);
        }
        Debug.DrawRay(transform.position, deltaMove * 3f, Color.green);
        transform.Translate(deltaMove);
        if (!movedObjs) {
            MoveObjectsAbove(deltaMove);
        }
        // Checks for ground and ceiling, resets jumps if grounded
        if (collisions.vHit) {
            if ((collisions.below && TotalSpeed.y < 0) || (collisions.above && TotalSpeed.y > 0)) {
                if (!collisions.onSlope || collisions.groundAngle < minWallAngle) {
                    speed.y = 0;
                    externalForce.y = 0;
                }
            }
        }
        gameObject.layer = layer;
        return deltaMove;
    }

    /// <summary>
    /// Tries to move any objects that are above this one
    /// </summary>
    /// <param name="deltaMove">How much to move each object</param>
    protected void MoveObjectsAbove(Vector2 deltaMove) {
        List<ObjectController2D> objsToMove = new List<ObjectController2D>();
        float rayLength = skinWidth * 2;
        float directionX = Mathf.Sign(deltaMove.x);
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = directionX == 1 ? raycastOrigins.topRight : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i) * -directionX;
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.up,
                rayLength, pConfig.standOnCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.cyan);
            foreach (RaycastHit2D hit in hits) {
                ObjectController2D obj = hit.transform.GetComponent<ObjectController2D>();
                if (obj && !objsToMove.Contains(obj) && hit.distance > 0) {
                    objsToMove.Add(obj);
                }
            }
        }
        foreach (ObjectController2D obj in objsToMove) {
            obj.Move(deltaMove);
        }
    }

}