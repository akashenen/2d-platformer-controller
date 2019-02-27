using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(Animator))]
/// <summary>
/// This is a component used alongside Actors that controls all the movement and physics operations
/// </summary>
public class Controller2D : MonoBehaviour {
    // Info that can be used in other classes
    public static readonly string GROUND_LAYER = "Ground";
    public static readonly string PLATFORM_LAYER = "Platform";
    public static readonly string PLAYER_LAYER = "Player";
    public static readonly string ENEMY_LAYER = "Enemy";
    public static readonly float GRAVITY = -50f;
    public static readonly float KNOCK_BACK_REDUCTION = 10f;
    // Colision parameters
    private static readonly float BOUNDS_EXPANSION = -2;
    private static readonly float RAY_COUNT = 4;
    public static readonly float SKIN_WIDTH = 0.015f;
    public static readonly float FALLTHROUGH_DELAY = 0.2f;
    public static readonly float MAX_CLIMB_ANGLE = 60f;
    // Animation attributes and names
    private static readonly string ANIMATION_H_SPEED = "hSpeed";
    private static readonly string ANIMATION_V_SPEED = "vSpeed";
    private static readonly string ANIMATION_JUMP = "jump";

    // Other Componenents
    private Actor actor;
    private BoxCollider2D myCollider;
    private Animator animator;

    // Physics properties
    private RaycastOrigins raycastOrigins;
    private CollisionInfo collisions;
    private float hSpeed = 0;
    private float vSpeed = 0;
    private float horizontalRaySpacing;
    private float verticalRaySpacing;
    private float gravityScale = 1;
    private bool ignorePlatformCollisions;

    // Public propoerties
    public bool FacingRight { get; set; } // false == left, true == right
    public bool OnLadder { get; set; }
    public bool KnockedBack { get; set; }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        actor = GetComponent<Actor>();
        myCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        KnockedBack = false;
        OnLadder = false;
        CalculateSpacing();
    }

    // Update is called once per frame
    void Update() {
        Move();
        HandleKnockback();
    }

    /// <summary>
    /// Calculates the spacing only once based on how many rays will be used
    /// </summary>
    void CalculateSpacing() {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(SKIN_WIDTH * BOUNDS_EXPANSION);
        horizontalRaySpacing = bounds.size.y / (RAY_COUNT - 1);
        verticalRaySpacing = bounds.size.x / (RAY_COUNT - 1);
    }

    /// <summary>
    /// The origin of each raycast must be updated every time before checking collisions
    /// </summary>
    void UpdateRaycastOrigins() {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(SKIN_WIDTH * BOUNDS_EXPANSION);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }

    /*-------------------------*/
    /*--------MOVEMENT---------*/
    /*-------------------------*/

    /// <summary>
    /// Tries to move according to current speed and checking for collisions
    /// </summary>
    public void Move() {
        UpdateRaycastOrigins();
        collisions.Reset();
        vSpeed += GRAVITY * gravityScale * Time.deltaTime;
        Vector2 velocity = new Vector2(hSpeed * Time.deltaTime, vSpeed * Time.deltaTime);
        float xDir = Mathf.Sign(velocity.x);
        CheckGround(xDir);
        if (velocity.x != 0) {
            // Slope checks and processing
            if (velocity.y < 0) {
                if (collisions.onGround) {
                    CheckNextSlope(xDir);
                } else if (collisions.nextSlopeAngle != 0) {
                    if (collisions.nextSlopeDirection == xDir)
                        SnapToSlope(ref velocity);
                }
                if (collisions.onSlope) {
                    if (collisions.groundDirection == xDir) {
                        DescendSlope(ref velocity);
                    } else {
                        ClimbSlope(ref velocity);
                    }
                }
            }
            HorizontalCollisions(ref velocity);
        }
        if (collisions.hHit && actor.canWallSlide && velocity.y < 0) {
            velocity.y = -actor.wallSlideVelocity * Time.deltaTime;
            vSpeed = -actor.wallSlideVelocity;
        }
        if (velocity.y > 0 || (velocity.y < 0 && (!collisions.onSlope || velocity.x == 0))) {
            VerticalCollisions(ref velocity);
        }
        if (collisions.onSlope) {
            CheckEndSlope(ref velocity);
        }
        Debug.DrawRay(transform.position, velocity * 3f, Color.green);
        transform.Translate(velocity);
        // Checks for ground and ceiling, resets jumps if grounded
        if (collisions.vHit) {
            if (collisions.below) {
                actor.extraJumps = actor.maxExtraJumps;
            }
            vSpeed = 0;
        }
        animator.SetFloat(ANIMATION_H_SPEED, hSpeed);
        animator.SetFloat(ANIMATION_V_SPEED, vSpeed);
    }

    /// <summary>
    /// Checks if actor is touching the ground, used to adjust to slopes
    /// </summary>
    /// <param name="dir">Direction the actor is moving, -1 = left, 1 = right</param>
    private void CheckGround(float dir) {
        Vector2 rayOrigin = dir == 1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
        for (int i = 0; i < RAY_COUNT; i++) {
            rayOrigin += (dir == 1 ? Vector2.right : Vector2.left) * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down,
                SKIN_WIDTH * 2f, LayerMask.GetMask(GROUND_LAYER, PLATFORM_LAYER));
            if (hit) {
                collisions.onGround = true;
                collisions.groundAngle = Vector2.Angle(hit.normal, Vector2.up);
                collisions.groundDirection = Mathf.Sign(hit.normal.x);
                break;
            }
        }
    }

    /// <summary>
    /// Checks for collisions in the horizontal axis and adjust the speed accordingly to stop at the 
    /// collided object.
    /// </summary>
    /// <param name="velocity">The current object velocity used for the raycast lenght</param>
    private void HorizontalCollisions(ref Vector2 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < RAY_COUNT; i++) {
            Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX,
                rayLength, LayerMask.GetMask(GROUND_LAYER));
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && !collisions.onSlope) {
                    collisions.onGround = true;
                    collisions.groundAngle = angle;
                    collisions.groundDirection = Mathf.Abs(hit.normal.x);
                    ClimbSlope(ref velocity);
                }
                if (!(i == 0 && collisions.onSlope)) {
                    if (angle > MAX_CLIMB_ANGLE) {
                        velocity.x = Mathf.Min(Mathf.Abs(velocity.x), (hit.distance - SKIN_WIDTH)) * directionX;
                        rayLength = Mathf.Min(Mathf.Abs(velocity.x) + SKIN_WIDTH, hit.distance);
                        if (collisions.onSlope) {
                            if (velocity.y < 0) {
                                velocity.y = 0;
                            } else {
                                velocity.y = Mathf.Tan(collisions.groundAngle * Mathf.Deg2Rad) *
                                    Mathf.Abs(velocity.x) * Mathf.Sign(velocity.y);
                            }
                        }
                        collisions.left = directionX < 0;
                        collisions.right = directionX > 0;
                        collisions.hHit = hit;
                        hSpeed = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks for collisions in the vertical axis and adjust the speed accordingly to stop at the 
    /// collided object.
    /// </summary>
    /// <param name="velocity">The current object velocity used for the raycast lenght</param>
    private void VerticalCollisions(ref Vector2 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < RAY_COUNT; i++) {
            Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                rayLength, LayerMask.GetMask(GROUND_LAYER));
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            // for jump-through platforms
            if (!ignorePlatformCollisions && directionY < 0 && !hit) {
                hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                    rayLength, LayerMask.GetMask(PLATFORM_LAYER));
            }
            if (hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                if (collisions.onSlope && directionY == 1) {
                    velocity.x = velocity.y / Mathf.Tan(collisions.groundAngle * Mathf.Deg2Rad) *
                        Mathf.Sign(velocity.x);
                    hSpeed = velocity.x / Time.deltaTime;
                }
                collisions.above = directionY > 0;
                collisions.below = directionY < 0;
                collisions.vHit = hit;
            }
        }
    }

    /// <summary>
    /// Tries to move the actor horizontally based on it's current movespeed and input pressure 
    /// while checking for movement impairments
    /// </summary>
    /// <param name="direction">-1 to 1; negative values = left; positive values = right</param>
    public void Walk(float direction) {
        if (CanMove()) {
            if (direction < 0)
                FacingRight = false;
            if (direction > 0)
                FacingRight = true;
            float acc = 0f;
            float dec = 0f;
            if (actor.advancedAirControl && !collisions.below) {
                acc = actor.airAccelerationTime;
                dec = actor.airDecelerationTime;
            } else {
                acc = actor.accelerationTime;
                dec = actor.decelerationTime;
            }
            if (acc > 0) {
                hSpeed += direction * (1 / acc) * actor.maxSpeed * Time.deltaTime;
            } else {
                hSpeed = actor.maxSpeed * direction;
            }
            if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(hSpeed)) {
                if (dec > 0) {
                    hSpeed = Mathf.MoveTowards(hSpeed, 0, (1 / dec) * actor.maxSpeed * Time.deltaTime);
                } else {
                    hSpeed = 0;
                }
            }
            hSpeed = Mathf.Clamp(hSpeed, -actor.maxSpeed, actor.maxSpeed);
        }
    }

    /// <summary>
    /// Adjusts to ascending a slope, transforming horizontal velocity into the angle of the slope
    /// </summary>
    /// <param name="velocity">The current actor velocity</param>
    private void ClimbSlope(ref Vector2 velocity) {
        if (collisions.groundAngle <= MAX_CLIMB_ANGLE) {
            float distance = Mathf.Abs(velocity.x);
            float yVelocity = Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad) * distance;
            if (velocity.y <= yVelocity) {
                velocity.y = yVelocity;
                velocity.x = Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad) * distance * Mathf.Sign(velocity.x);
                collisions.below = true;
                vSpeed = 0;
            }
        } else {
            collisions.groundAngle = 0;
        }
    }

    /// <summary>
    /// Adjusts to descending a slope, transforming horizontal velocity into the angle of the slope
    /// </summary>
    /// <param name="velocity">The current actor velocity</param>
    private void DescendSlope(ref Vector2 velocity) {
        if (collisions.groundAngle <= MAX_CLIMB_ANGLE) {
            float distance = Mathf.Abs(velocity.x);
            velocity.x = (Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad) * distance) * Mathf.Sign(velocity.x);
            velocity.y = -Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad) * distance;
            collisions.below = true;
            vSpeed = 0;
        } else {
            collisions.groundAngle = 0;
        }
    }

    /// <summary>
    /// Tries to find a slope ahead of the actor, so it can snap to the slope once it leaves the ground
    /// </summary>
    /// <param name="dir">Direction the actor is moving, -1 = left, 1 = right</param>
    private void CheckNextSlope(float dir) {
        Vector2 rayOrigin = dir == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, LayerMask.GetMask(GROUND_LAYER, PLATFORM_LAYER));
        Debug.DrawRay(rayOrigin, Vector2.down, Color.blue);
        if (hit) {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle > 0 && angle <= MAX_CLIMB_ANGLE) {
                collisions.nextSlopeAngle = angle;
                collisions.nextSlopeDirection = Mathf.Sign(hit.normal.x);
            } else {
                collisions.nextSlopeAngle = 0;
                collisions.nextSlopeDirection = 0;
            }
        }
    }

    /// <summary>
    /// Snaps to a slope when leaving ground due to horizontal speed, preventing the actor to float off slopes
    /// </summary>
    /// <param name="velocity">The current actor velocity</param>
    private void SnapToSlope(ref Vector2 velocity) {
        float dir = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = dir == -1 ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity,
            LayerMask.GetMask(GROUND_LAYER, PLATFORM_LAYER));
        if (hit && hit.distance < 1f) {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle == collisions.nextSlopeAngle) {
                collisions.groundAngle = angle;
                collisions.groundDirection = Mathf.Sign(hit.normal.x);
                collisions.onGround = true;
                transform.Translate(0, -(hit.distance - SKIN_WIDTH), 0);
            } else {
                collisions.nextSlopeAngle = 0;
                collisions.nextSlopeDirection = 0;
            }
        }
    }

    /// <summary>
    /// Checks for slope ends and angle changes, preventing the actor from briefly passing through ground and losing velocity
    /// </summary>
    /// <param name="velocity">The current actor velocity</param>
    private void CheckEndSlope(ref Vector2 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        if (velocity.y > 0) {
            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
            Vector2 rayOrigin = (directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) +
                Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left * directionX, rayLength, LayerMask.GetMask(GROUND_LAYER));
            if (hit) {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle != collisions.groundAngle) {
                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                    collisions.groundAngle = angle;
                }
            }
        } else {
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
            Vector2 rayOrigin = (directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) +
                Vector2.left * velocity.x;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, LayerMask.GetMask(GROUND_LAYER, PLATFORM_LAYER));
            if (hit) {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle != collisions.groundAngle) {
                    velocity.y = -(hit.distance - SKIN_WIDTH);
                    collisions.groundAngle = angle;
                }
            }
        }
    }

    /// <summary>
    ///  Makes the actor jump if possible
    /// </summary>
    public void Jump() {
        if (CanMove()) {
            if (collisions.below || actor.extraJumps > 0 || (actor.canWallJump && collisions.hHit)) {
                vSpeed = Mathf.Sqrt(2 * Mathf.Abs(GRAVITY) * actor.jumpHeight);
                animator.SetTrigger(ANIMATION_JUMP);
                // wall jump
                if (actor.canWallJump && collisions.hHit && !collisions.below && !collisions.onSlope) {
                    hSpeed = collisions.left ? actor.wallJumpVelocity : -actor.wallJumpVelocity;
                    actor.extraJumps = actor.maxExtraJumps;
                } else {
                    // air jump
                    if (!collisions.below)
                        actor.extraJumps--;
                }
                RestorePlatformCollisions();
            }
        }
    }

    /// <summary>
    /// If the actor is standing on a platform, will ignore platforms briefly,
    /// otherwise it will just jump
    /// </summary>
    public void JumpDown() {
        if (CanMove()) {
            if (collisions.vHit &&
                collisions.vHit.collider.gameObject.layer == LayerMask.NameToLayer(PLATFORM_LAYER)) {
                ignorePlatformCollisions = true;
                Invoke("RestorePlatformCollisions", FALLTHROUGH_DELAY);
            } else {
                Jump();
            }
        }
    }

    /// <summary>
    /// Restores plaftorm collisions after the set amount of time has passed
    /// or the actor has jumped
    /// </summary>
    private void RestorePlatformCollisions() {
        ignorePlatformCollisions = false;
    }

    /// <summary>
    /// Used to alter gravity strength for jump hold or other effects
    /// </summary>
    /// <param name="gravityScale">Desired gravity scale</param>
    public void SetGravityScale(float gravityScale) {
        this.gravityScale = gravityScale;
    }

    /// <summary>
    /// Handles knockback force and movement
    /// </summary>
    public void HandleKnockback() {
        if (KnockedBack) {
            float directionX = Mathf.Sign(hSpeed);
            float newHSpeed = Mathf.Abs(hSpeed) - (KNOCK_BACK_REDUCTION * Time.deltaTime);
            if (newHSpeed <= 0) {
                newHSpeed = 0;
                KnockedBack = false;
            }
            hSpeed = newHSpeed * directionX;
        }
    }

    /// <summary>
    /// Checks if there are any knockbacks or status that stop movement
    /// </summary>
    /// <returns>Whether the actor can move or not</returns>
    public bool CanMove() {
        return (!KnockedBack);
    }

    // Used to store temporary locations of raycast origins (the corners of the collider)
    struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    // Stores temporary collision info to be used during calculations
    struct CollisionInfo {
        public bool above, below, left, right;
        public RaycastHit2D hHit, vHit;
        public bool onGround;
        public float groundAngle;
        public float groundDirection;
        public float nextSlopeAngle;
        public float nextSlopeDirection;
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