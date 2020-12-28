using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ObjectController2D : MonoBehaviour {
    // Colision parameters
    public float raySpacing = 0.125f;
    public float skinWidth = 0.015f;
    public float owPlatformDelay = 0.1f;
    public float ladderClimbThreshold = 0.3f;
    public float ladderDelay = 0.3f;
    public float maxSlopeAngle = 60f;
    public float minWallAngle = 80f;

    protected RaycastOrigins raycastOrigins;
    protected CollisionInfo collisions;
    protected BoxCollider2D myCollider;
    protected PhysicsConfig pConfig;
    [SerializeField]
    protected Vector2 speed = Vector2.zero;
    [SerializeField]
    protected Vector2 externalForce = Vector2.zero;
    protected float horizontalRaySpacing;
    protected float horizontalRayCount;
    protected float verticalRaySpacing;
    protected float verticalRayCount;
    protected float gravityScale = 1;
    protected LayerMask collisionMask;
    protected float ignorePlatformsTime = 0;

    protected float minimumMoveThreshold = 0.01f;

    public bool FacingRight { get; set; } // false == left, true == right
    public bool IgnoreFriction { get; set; }
    public Vector2 TotalSpeed => speed + externalForce;

    // Start is called before the first frame update
    public virtual void Start() {
        myCollider = GetComponent<BoxCollider2D>();
        CalculateSpacing();
        pConfig = GameObject.FindObjectOfType<PhysicsConfig>();
        if (!pConfig) {
            pConfig = (PhysicsConfig) new GameObject().AddComponent(typeof(PhysicsConfig));
            pConfig.gameObject.name = "Physics Config";
            Debug.LogWarning("PhysicsConfig not found on the scene! Using default config.");
        }
        collisionMask = pConfig.characterCollisionMask;
        FacingRight = true;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    public virtual void FixedUpdate() {
        collisions.Reset();
        Move((TotalSpeed) * Time.fixedDeltaTime);
        PostMove();
    }

    /// <summary>
    /// Calculates the spacing only once based on how many rays will be used
    /// </summary>
    void CalculateSpacing() {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(skinWidth * -2);
        horizontalRayCount = Mathf.Round(bounds.size.y / raySpacing);
        verticalRayCount = Mathf.Round(bounds.size.x / raySpacing);
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    /// <summary>
    /// The origin of each raycast must be updated every time before checking collisions
    /// </summary>
    protected void UpdateRaycastOrigins() {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(skinWidth * -2);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    /// <summary>
    /// Updates the character's vertical speed according to gravity, gravity scale and other properties
    /// </summary>
    protected virtual void UpdateGravity() {
        float g = pConfig.gravity * gravityScale * Time.fixedDeltaTime;
        if (speed.y > 0) {
            speed.y += g;
        } else {
            externalForce.y += g;
        }
    }

    /// <summary>
    /// Reduces the external force over time according to the air or ground frictions
    /// </summary>
    protected virtual void UpdateExternalForce() {
        if (IgnoreFriction) {
            return;
        }
        float friction = collisions.onGround ? pConfig.groundFriction : pConfig.airFriction;
        externalForce = Vector2.MoveTowards(externalForce, Vector2.zero,
            externalForce.magnitude * friction * Time.fixedDeltaTime);
        if(externalForce.magnitude <= minimumMoveThreshold) {
            externalForce = Vector2.zero;
        }
    }

    protected virtual void PreMove(ref Vector2 deltaMove) {
        UpdateRaycastOrigins();
        float xDir = Mathf.Sign(deltaMove.x);
        CheckGround(xDir);
        UpdateExternalForce();
        UpdateGravity();
        if (collisions.onSlope && collisions.groundAngle > maxSlopeAngle &&
            (collisions.groundAngle < minWallAngle || speed.x == 0)) {
            externalForce.x += -pConfig.gravity * pConfig.groundFriction * collisions.groundDirection * Time.fixedDeltaTime / 4;
        }
    }

    /// <summary>
    /// Tries to move according to current speed and checking for collisions
    /// </summary>
    public virtual Vector2 Move(Vector2 deltaMove) {
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
        if (deltaMove.y > 0 || (deltaMove.y < 0 && (!collisions.onSlope || deltaMove.x == 0))) {
            VerticalCollisions(ref deltaMove);
        }
        if (collisions.onGround && deltaMove.x != 0 && speed.y <= 0) {
            HandleSlopeChange(ref deltaMove);
        }
        Debug.DrawRay(transform.position, deltaMove * 3f, Color.green);
        transform.Translate(deltaMove);
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
    /// Checks if character is touching the ground, used to adjust to slopes
    /// </summary>
    /// <param name="direction">Direction the character is moving, -1 = left, 1 = right</param>
    protected void CheckGround(float direction) {
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = direction == 1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += (direction == 1 ? Vector2.right : Vector2.left) * (verticalRaySpacing * i);
            rayOrigin.y += skinWidth * 2;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down,
                skinWidth * 4f, collisionMask);
            if (!hit && ignorePlatformsTime <= 0) {
                hit = Physics2D.Raycast(rayOrigin, Vector2.down,
                    skinWidth * 4f, pConfig.owPlatformMask);
                if (hit.distance <= 0) {
                    continue;
                }
            }
            if (hit) {
                collisions.onGround = true;
                collisions.groundAngle = Vector2.Angle(hit.normal, Vector2.up);
                collisions.groundDirection = Mathf.Sign(hit.normal.x);
                collisions.groundLayer = hit.collider.gameObject.layer;
                collisions.vHit = hit;
                collisions.below = true;
                Debug.DrawRay(rayOrigin, Vector2.down * skinWidth * 2, Color.blue);
                break;
            }
        }
    }

    /// <summary>
    /// Checks for collisions in the horizontal axis and adjust the speed accordingly to stop at the 
    /// collided object.
    /// </summary>
    /// <param name="deltaMove">The current object deltaMove used for the raycast lenght</param>
    protected void HorizontalCollisions(ref Vector2 deltaMove) {
        float directionX = Mathf.Sign(deltaMove.x);
        float rayLength = Mathf.Abs(deltaMove.x) + skinWidth;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX,
                rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && !collisions.onSlope && angle < minWallAngle) {
                    collisions.onGround = true;
                    collisions.groundAngle = angle;
                    collisions.groundDirection = Mathf.Sign(hit.normal.x);
                    deltaMove.x -= (hit.distance - skinWidth) * directionX;
                    ClimbSlope(ref deltaMove);
                    deltaMove.x += (hit.distance - skinWidth) * directionX;
                    rayLength = Mathf.Min(Mathf.Abs(deltaMove.x) + skinWidth, hit.distance);
                }
                if (!(i == 0 && collisions.onSlope)) {
                    if (angle > maxSlopeAngle) {
                        if (angle < minWallAngle) {
                            continue;
                        }
                        deltaMove.x = Mathf.Min(Mathf.Abs(deltaMove.x), (hit.distance - skinWidth)) * directionX;
                        rayLength = Mathf.Min(Mathf.Abs(deltaMove.x) + skinWidth, hit.distance);
                        if (collisions.onSlope && collisions.groundAngle < minWallAngle) {
                            if (deltaMove.y < 0) {
                                deltaMove.y = 0;
                            } else {
                                deltaMove.y = Mathf.Tan(collisions.groundAngle * Mathf.Deg2Rad) *
                                    Mathf.Abs(deltaMove.x) * Mathf.Sign(deltaMove.y);
                            }
                        }
                        collisions.left = directionX < 0;
                        collisions.right = directionX > 0;
                        collisions.hHit = hit;
                        speed.x = 0;
                        externalForce.x = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks for collisions in the vertical axis and adjust the speed accordingly to stop at the 
    /// collided object.
    /// </summary>
    /// <param name="deltaMove">The current object deltaMove used for the raycast lenght</param>
    protected virtual void VerticalCollisions(ref Vector2 deltaMove) {
        float directionY = Mathf.Sign(deltaMove.y);
        float rayLength = Mathf.Abs(deltaMove.y) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + deltaMove.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (directionY < 0 && !hit) {
                hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                    rayLength, pConfig.owPlatformMask);
            }
            if (hit) {
                deltaMove.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
                if (collisions.onSlope && directionY == 1) {
                    deltaMove.x = deltaMove.y / Mathf.Tan(collisions.groundAngle * Mathf.Deg2Rad) *
                        Mathf.Sign(deltaMove.x);
                    speed.x = 0;
                    externalForce.x = 0;
                }
                collisions.above = directionY > 0;
                collisions.below = directionY < 0;
                collisions.vHit = hit;

            }
        }
    }

    /// <summary>
    /// Adjusts to ascending a slope, transforming horizontal deltaMove into the angle of the slope
    /// </summary>
    /// <param name="deltaMove">The current character deltaMove</param>
    protected void ClimbSlope(ref Vector2 deltaMove) {
        if (collisions.groundAngle < minWallAngle) {
            float distance = Mathf.Abs(deltaMove.x);
            float yMove = Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad) * distance;
            if (deltaMove.y <= yMove) {
                deltaMove.y = yMove;
                deltaMove.x = Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad) * distance * Mathf.Sign(deltaMove.x);
                collisions.below = true;
                speed.y = 0;
                externalForce.y = 0;
            }
        }
    }

    /// <summary>
    /// Adjusts to descending a slope, transforming horizontal deltaMove into the angle of the slope
    /// </summary>
    /// <param name="deltaMove">The current character deltaMove</param>
    protected void DescendSlope(ref Vector2 deltaMove) {
        float distance = Mathf.Abs(deltaMove.x);
        deltaMove.x = (Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad) * distance) * Mathf.Sign(deltaMove.x);
        deltaMove.y = -Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad) * distance;
        collisions.below = true;
        speed.y = 0;
        externalForce.y = 0;
    }

    /// <summary>
    /// Checks for angle changes on the ground, preventing the character from briefly passing through ground 
    /// and losing deltaMove or leaving the ground and floating (lots of trigonometry)
    /// </summary>
    /// <param name="deltaMove">The current character deltaMove</param>
    protected virtual void HandleSlopeChange(ref Vector2 deltaMove) {
        float directionX = Mathf.Sign(deltaMove.x);
        if (deltaMove.y > 0) {
            // climb steeper slope
            float rayLength = Mathf.Abs(deltaMove.x) + skinWidth * 2;
            Vector2 rayOrigin = (directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) +
                Vector2.up * deltaMove.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit) {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle != collisions.groundAngle) {
                    deltaMove.x = (hit.distance - skinWidth) * directionX;
                    collisions.groundAngle = angle;
                    collisions.groundDirection = Mathf.Sign(hit.normal.x);
                }
            } else {
                // climb milder slope or flat ground
                rayOrigin = (directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + deltaMove;
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.down, Color.yellow);
                if (hit && hit.collider.gameObject.layer == collisions.groundLayer) {
                    float angle = Vector2.Angle(hit.normal, Vector2.up);
                    float overshoot = 0;
                    if (angle < collisions.groundAngle) {
                        if (angle > 0) {
                            float tanA = Mathf.Tan(angle * Mathf.Deg2Rad);
                            float tanB = Mathf.Tan(collisions.groundAngle * Mathf.Deg2Rad);
                            float sin = Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad);
                            overshoot = (2 * tanA * hit.distance - tanB * hit.distance) /
                                (tanA * sin - tanB * sin);
                        } else {
                            overshoot = hit.distance / Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad);
                        }
                        float removeX = Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float removeY = Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad) * overshoot;
                        float addX = Mathf.Cos(angle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float addY = Mathf.Sin(angle * Mathf.Deg2Rad) * overshoot;
                        deltaMove += new Vector2(addX - removeX, addY - removeY + skinWidth);
                    }
                }
            }
        } else {
            // descend milder slope or flat ground
            float rayLength = Mathf.Abs(deltaMove.y) + skinWidth;
            Vector2 rayOrigin = (directionX == -1 ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft) +
                Vector2.right * deltaMove.x;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, collisionMask);
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (hit && angle < collisions.groundAngle) {
                deltaMove.y = -(hit.distance - skinWidth);
                collisions.groundAngle = angle;
                collisions.groundDirection = Mathf.Sign(hit.normal.x);
            } else {
                // descend steeper slope
                rayOrigin = (directionX == 1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + deltaMove;
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.down, Color.yellow);
                if (hit && Mathf.Sign(hit.normal.x) == directionX &&
                    hit.collider.gameObject.layer == collisions.groundLayer) {
                    angle = Vector2.Angle(hit.normal, Vector2.up);
                    float overshoot = 0;
                    if (angle > collisions.groundAngle && Mathf.Sign(hit.normal.x) == (FacingRight ? 1 : -1)) {
                        if (collisions.groundAngle > 0) {
                            float sin = Mathf.Sin((collisions.groundAngle) * Mathf.Deg2Rad);
                            float cos = Mathf.Cos((collisions.groundAngle) * Mathf.Deg2Rad);
                            float tan = Mathf.Tan(angle * Mathf.Deg2Rad);
                            overshoot = hit.distance * cos / (tan / cos - sin);
                        } else {
                            overshoot = hit.distance / Mathf.Tan(angle * Mathf.Deg2Rad);
                        }
                        float removeX = Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float removeY = -Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad) * overshoot;
                        float addX = Mathf.Cos(angle * Mathf.Deg2Rad) * overshoot * Mathf.Sign(deltaMove.x);
                        float addY = -Mathf.Sin(angle * Mathf.Deg2Rad) * overshoot;
                        deltaMove += new Vector2(addX - removeX, addY - removeY - skinWidth);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Is called after moving
    /// </summary>
    protected void PostMove() {
        IgnoreFriction = false;
    }

    /// <summary>
    /// Adds the specified force to the character's total external force
    /// </summary>
    /// <param name="force">Force to be added</param>
    public void ApplyForce(Vector2 force) {
        externalForce += force;
    }

    /// <summary>
    /// Sets the character's external force to the specified amount
    /// </summary>
    /// <param name="force">Force to be set</param>
    public virtual void SetForce(Vector2 force) {
        externalForce = force;
        // resets gravity
        if (speed.y < 0) {
            speed.y = 0;
        }
    }

    /// <summary>
    /// Used to alter gravity strength
    /// </summary>
    /// <param name="gravityScale">Desired gravity scale</param>
    public void SetGravityScale(float gravityScale) {
        this.gravityScale = gravityScale;
    }

    // Used to store temporary locations of raycast origins (the corners of the collider)
    protected struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    // Stores temporary collision info to be used during calculations
    protected struct CollisionInfo {
        public bool above, below, left, right;
        public RaycastHit2D hHit, vHit;
        public bool onGround;
        public float groundAngle;
        public float groundDirection;
        public int groundLayer;
        public bool onSlope { get { return onGround && groundAngle != 0; } }

        public void Reset() {
            above = false;
            below = false;
            left = false;
            right = false;
            hHit = new RaycastHit2D();
            vHit = new RaycastHit2D();
            onGround = false;
            groundAngle = 0;
            groundDirection = 0;
        }
    }
}