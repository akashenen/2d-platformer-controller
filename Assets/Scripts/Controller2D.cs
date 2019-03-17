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
    // Colision parameters
    public float raySpacing = 0.25f;
    public float skinWidth = 0.015f;
    public float owPlatformDelay = 0.2f;
    public float ladderClimbThreshold = 0.3f;
    public float ladderDelay = 0.3f;
    public float maxSlopeAngle = 60f;
    public float minWallAngle = 80f;
    // Animation attributes and names
    private static readonly string ANIMATION_H_SPEED = "hSpeed";
    private static readonly string ANIMATION_V_SPEED = "vSpeed";
    private static readonly string ANIMATION_JUMP = "jump";
    private static readonly string ANIMATION_GROUNDED = "grounded";
    private static readonly string ANIMATION_DASHING = "dashing";
    private static readonly string ANIMATION_WALL = "onWall";
    private static readonly string ANIMATION_FACING = "facingRight";
    private static readonly string ANIMATION_LADDER = "onLadder";

    // Other Componenents
    private Actor actor;
    private BoxCollider2D myCollider;
    private Animator animator;
    private PhysicsConfig pConfig;

    // Physics properties
    private RaycastOrigins raycastOrigins;
    private CollisionInfo collisions;
    [SerializeField]
    private Vector2 speed = Vector2.zero;
    [SerializeField]
    private Vector2 externalForce = Vector2.zero;
    private float horizontalRaySpacing;
    private float horizontalRayCount;
    private float verticalRaySpacing;
    private float verticalRayCount;
    private float gravityScale = 1;
    private float ignorePlatforms = 0;
    private float ignoreLadders = 0;
    private int extraJumps = 0;
    private int airDashes = 0;
    private float dashCooldown = 0;
    private float dashStaggerTime = 0;
    private float ladderX = 0;

    // Public propoerties
    public bool FacingRight { get; set; } // false == left, true == right
    public bool OnLadder { get; set; }
    public bool KnockedBack { get; set; }
    public bool Dashing { get; set; }
    public Vector2 TotalSpeed { get { return speed + externalForce; } }

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
        Dashing = false;
        CalculateSpacing();
        pConfig = GameObject.FindObjectOfType<PhysicsConfig>();
        if (!pConfig) {
            pConfig = (PhysicsConfig) new GameObject().AddComponent(typeof(PhysicsConfig));
            pConfig.gameObject.name = "Physics Config";
            Debug.LogWarning("PhysicsConfig not found on the scene! Using default config.");
        }
    }

    /// <summary>
    /// Update is called once pre frame
    /// </summary>
    void Update() {
        UpdateTimers();
        UpdateDash();
        UpdateKnockback();
        UpdateExternalForce();
        UpdateGravity();
        collisions.Reset();
        Move((TotalSpeed) * Time.deltaTime);
        SetAnimations();
    }

    /// <summary>
    /// Calculates the spacing only once based on how many rays will be used
    /// </summary>
    void CalculateSpacing() {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(skinWidth * -2);
        horizontalRayCount = Mathf.Round(bounds.size.x / raySpacing);
        verticalRayCount = Mathf.Round(bounds.size.y / raySpacing);
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    /// <summary>
    /// The origin of each raycast must be updated every time before checking collisions
    /// </summary>
    void UpdateRaycastOrigins() {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(skinWidth * -2);
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
    public void Move(Vector2 deltaMove) {
        UpdateRaycastOrigins();
        float xDir = Mathf.Sign(deltaMove.x);
        CheckGround(xDir);
        if (deltaMove.x != 0) {
            // Slope checks and processing
            if (deltaMove.y <= 0 && actor.canUseSlopes) {
                if (collisions.onSlope) {
                    if (collisions.groundDirection == xDir) {
                        if ((!Dashing && dashStaggerTime <= 0) || actor.dashDownSlopes) {
                            DescendSlope(ref deltaMove);
                        }
                    } else {
                        ClimbSlope(ref deltaMove);
                    }
                }
            }
            HorizontalCollisions(ref deltaMove);
        }
        if (collisions.onSlope && collisions.groundAngle >= minWallAngle && collisions.groundDirection != xDir &&
            speed.y < 0) {
            float sin = Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad);
            deltaMove.x = cos * actor.wallSlideSpeed * Time.deltaTime * collisions.groundDirection;
            deltaMove.y = sin * -actor.wallSlideSpeed * Time.deltaTime;
            speed.y = -actor.wallSlideSpeed;
            speed.x = 0;
            Vector2 origin = collisions.groundDirection == -1 ? raycastOrigins.bottomRight : raycastOrigins.bottomRight;
            collisions.hHit = Physics2D.Raycast(origin, Vector2.left * collisions.groundDirection,
                1f, pConfig.groundMask);
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
                if (collisions.below) {
                    ResetJumpsAndDashes();
                }
                if (!collisions.onSlope || collisions.groundAngle < minWallAngle) {
                    speed.y = 0;
                    externalForce.y = 0;
                }
            }
        }
    }

    /// <summary>
    /// Updates the actor's vertical speed according to gravity, gravity scale and other properties
    /// </summary>
    private void UpdateGravity() {
        if (!OnLadder && !Dashing && dashStaggerTime <= 0) {
            speed.y += pConfig.gravity * gravityScale * Time.deltaTime;
        }
        if (collisions.hHit && actor.canWallSlide && TotalSpeed.y <= 0) {
            externalForce.y = 0;
            speed.y = -actor.wallSlideSpeed;
        }
    }

    /// <summary>
    /// Adds the specified force to the actor's total external force
    /// </summary>
    /// <param name="force">Force to be added</param>
    public void ApplyForce(Vector2 force) {
        externalForce += force;
    }

    /// <summary>
    /// Sets the actor's external force to the specified amount
    /// </summary>
    /// <param name="force">Force to be set</param>
    public void SetForce(Vector2 force) {
        externalForce = force;
        // resets gravity
        if (speed.y < 0) {
            speed.y = 0;
        }
        // cancels dash
        Dashing = false;
        dashStaggerTime = 0;
    }

    /// <summary>
    /// Reduces the external force over time according to the air or ground frictions
    /// </summary>
    private void UpdateExternalForce() {
        float friction = collisions.onGround ? pConfig.groundFriction : pConfig.airFriction;
        if (!Dashing && dashStaggerTime <= 0) {
            externalForce = Vector2.MoveTowards(externalForce, Vector2.zero, friction * Time.deltaTime);
        }
    }

    /// <summary>
    /// Updates the actor's animator with the movement and collision values
    /// </summary>
    private void SetAnimations() {
        animator.SetFloat(ANIMATION_H_SPEED, speed.x + externalForce.x);
        animator.SetFloat(ANIMATION_V_SPEED, speed.y + externalForce.y);
        animator.SetBool(ANIMATION_GROUNDED, collisions.onGround);
        animator.SetBool(ANIMATION_DASHING, Dashing);
        animator.SetBool(ANIMATION_WALL, collisions.hHit);
        animator.SetBool(ANIMATION_FACING, FacingRight);
        animator.SetBool(ANIMATION_LADDER, OnLadder);
    }

    /// <summary>
    /// Checks if actor is touching the ground, used to adjust to slopes
    /// </summary>
    /// <param name="direction">Direction the actor is moving, -1 = left, 1 = right</param>
    private void CheckGround(float direction) {
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = direction == 1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += (direction == 1 ? Vector2.right : Vector2.left) * (verticalRaySpacing * i);
            rayOrigin.y += skinWidth * 2;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down,
                skinWidth * 4f, pConfig.allPlatformsMask);
            if (hit) {
                collisions.onGround = true;
                collisions.groundAngle = Vector2.Angle(hit.normal, Vector2.up);
                collisions.groundDirection = Mathf.Sign(hit.normal.x);
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
    private void HorizontalCollisions(ref Vector2 deltaMove) {
        float directionX = Mathf.Sign(deltaMove.x);
        float rayLength = Mathf.Abs(deltaMove.x) + skinWidth;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX,
                rayLength, pConfig.groundMask);
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
    private void VerticalCollisions(ref Vector2 deltaMove) {
        if (OnLadder) {
            Vector2 origin = myCollider.bounds.center + Vector3.up *
                (myCollider.bounds.extents.y * Mathf.Sign(deltaMove.y) + deltaMove.y);
            Collider2D hit = Physics2D.OverlapCircle(origin, 0, pConfig.groundMask);
            if (!hit) {
                return;
            }
            hit = Physics2D.OverlapCircle(origin, 0, pConfig.ladderMask);
            if (hit) {
                return;
            }
        }
        float directionY = Mathf.Sign(deltaMove.y);
        float rayLength = Mathf.Abs(deltaMove.y) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + deltaMove.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                rayLength, pConfig.groundMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            // for one way platforms
            if (ignorePlatforms <= 0 && directionY < 0 && !hit) {
                hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY,
                    rayLength, pConfig.owPlatformMask);
            }
            if (hit) {
                deltaMove.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
                if (OnLadder && directionY < 0) {
                    OnLadder = false;
                    IgnoreLadders();
                }
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
    /// Tries to move the actor horizontally based on it's current movespeed and input pressure 
    /// while checking for movement impairments
    /// </summary>
    /// <param name="direction">-1 to 1; negative values = left; positive values = right</param>
    public void Walk(float direction) {
        if (collisions.onSlope && collisions.groundAngle > maxSlopeAngle &&
            (collisions.groundAngle < minWallAngle || direction == 0)) {
            direction = collisions.groundDirection;
            //speed.x = Mathf.Max(Mathf.Abs(speed.x), Mathf.Abs(speed.y)) * direction;
        }
        if (CanMove() && !Dashing && dashStaggerTime <= 0) {
            if (direction < 0)
                FacingRight = false;
            if (direction > 0)
                FacingRight = true;
            if (OnLadder) {
                return;
            }
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
                if (Mathf.Abs(speed.x) < actor.maxSpeed) {
                    speed.x += direction * (1 / acc) * actor.maxSpeed * Time.deltaTime;
                    speed.x = Mathf.Min(Mathf.Abs(speed.x), actor.maxSpeed) * Mathf.Sign(speed.x);
                }
            } else {
                speed.x = actor.maxSpeed * direction;
            }
            if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(speed.x)) {
                if (dec > 0) {
                    speed.x = Mathf.MoveTowards(speed.x, 0, (1 / dec) * actor.maxSpeed * Time.deltaTime);
                } else {
                    speed.x = 0;
                }
            }
        }
    }

    /// <summary>
    /// Adjusts to ascending a slope, transforming horizontal deltaMove into the angle of the slope
    /// </summary>
    /// <param name="deltaMove">The current actor deltaMove</param>
    private void ClimbSlope(ref Vector2 deltaMove) {
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
    /// <param name="deltaMove">The current actor deltaMove</param>
    private void DescendSlope(ref Vector2 deltaMove) {
        float distance = Mathf.Abs(deltaMove.x);
        deltaMove.x = (Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad) * distance) * Mathf.Sign(deltaMove.x);
        deltaMove.y = -Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad) * distance;
        collisions.below = true;
        speed.y = 0;
        externalForce.y = 0;
    }

    /// <summary>
    /// Checks for angle changes on the ground, preventing the actor from briefly passing through ground 
    /// and losing deltaMove or leaving the ground and floating (lots of trigonometry)
    /// </summary>
    /// <param name="deltaMove">The current actor deltaMove</param>
    private void HandleSlopeChange(ref Vector2 deltaMove) {
        float directionX = Mathf.Sign(deltaMove.x);
        if (deltaMove.y > 0) {
            // climb steeper slope
            float rayLength = Mathf.Abs(deltaMove.x) + skinWidth * 2;
            Vector2 rayOrigin = (directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) +
                Vector2.up * deltaMove.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, pConfig.groundMask);
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
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, pConfig.allPlatformsMask);
                Debug.DrawRay(rayOrigin, Vector2.down, Color.yellow);
                if (hit) {
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
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, pConfig.allPlatformsMask);
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (hit && angle < collisions.groundAngle) {
                deltaMove.y = -(hit.distance - skinWidth);
                collisions.groundAngle = angle;
                collisions.groundDirection = Mathf.Sign(hit.normal.x);
            } else {
                // descend steeper slope
                if ((Dashing || dashStaggerTime > 0) && !actor.dashDownSlopes) {
                    return;
                }
                rayOrigin = (directionX == 1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + deltaMove;
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1f, pConfig.allPlatformsMask);
                Debug.DrawRay(rayOrigin, Vector2.down, Color.yellow);
                if (hit && Mathf.Sign(hit.normal.x) == directionX) {
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
    ///  Makes the actor jump if possible
    /// </summary>
    public void Jump() {
        if (CanMove() && (!Dashing || actor.canJumpDuringDash)) {
            if (collisions.onGround || extraJumps > 0 || (actor.canWallJump && collisions.hHit)) {
                // air jump
                if (!collisions.below && !OnLadder)
                    extraJumps--;
                float height = actor.maxJumpHeight;
                if (OnLadder) {
                    Vector2 origin = myCollider.bounds.center + Vector3.up * myCollider.bounds.extents.y;
                    Collider2D hit = Physics2D.OverlapCircle(origin, 0, pConfig.groundMask);
                    if (hit) {
                        return;
                    }
                    origin = myCollider.bounds.center + Vector3.down * myCollider.bounds.extents.y;
                    hit = Physics2D.OverlapCircle(origin, 0, pConfig.groundMask);
                    if (hit) {
                        return;
                    }
                    height = actor.ladderJumpHeight;
                    externalForce.x += actor.ladderJumpSpeed * (FacingRight ? 1 : -1);
                    OnLadder = false;
                    IgnoreLadders();
                    ResetJumpsAndDashes();
                }
                speed.y = Mathf.Sqrt(-2 * pConfig.gravity * height);
                externalForce.y = 0;
                animator.SetTrigger(ANIMATION_JUMP);
                if (actor.jumpCancelStagger) {
                    dashStaggerTime = 0;
                }
                // wall jump
                if (actor.canWallJump && collisions.hHit && !collisions.below) {
                    externalForce.x += collisions.left ? actor.wallJumpSpeed : -actor.wallJumpSpeed;
                    ResetJumpsAndDashes();
                }
                // slope sliding jump
                if (collisions.onSlope && collisions.groundAngle > maxSlopeAngle &&
                    collisions.groundAngle < minWallAngle) {
                    speed.x = actor.maxSpeed * collisions.groundDirection;
                }
                ignorePlatforms = 0;
            }
        }
    }

    /// <summary>
    /// Ends the jump ealier by setting the vertical speed to the minimum jump speed if it's higher
    /// </summary>
    public void EndJump() {
        float yMove = Mathf.Sqrt(-2 * pConfig.gravity * actor.minJumpHeight);
        if (speed.y > yMove) {
            speed.y = yMove;
        }
    }

    /// <summary>
    /// Makes the actor dash in the specified direction if possible.
    /// If omnidirectional dash is disabled, will only dash in the horizontal axis
    /// </summary>
    /// <param name="direction">The desired direction of the dash</param>
    public void Dash(Vector2 direction) {
        if (CanMove() && actor.canDash && dashCooldown <= 0) {
            if (OnLadder) {
                Vector2 origin = myCollider.bounds.center + Vector3.up * myCollider.bounds.extents.y;
                Collider2D hit = Physics2D.OverlapCircle(origin, 0, pConfig.groundMask);
                if (hit) {
                    return;
                }
                origin = myCollider.bounds.center + Vector3.down * myCollider.bounds.extents.y;
                hit = Physics2D.OverlapCircle(origin, 0, pConfig.groundMask);
                if (hit) {
                    return;
                }
                OnLadder = false;
            }
            if (!collisions.onGround) {
                if (airDashes > 0) {
                    airDashes--;
                } else {
                    return;
                }
            }
            Dashing = true;
            if (direction.magnitude == 0 || (collisions.onGround && direction.y < 0)) {
                direction = FacingRight ? Vector2.right : Vector2.left;
            }
            // wall dash
            if (collisions.hHit) {
                direction = FacingRight ? Vector2.left : Vector2.right;
                ResetJumpsAndDashes();
            }
            if (!actor.omnidirectionalDash) {
                direction = Vector2.right * Mathf.Sign(direction.x);
            }
            direction = direction.normalized * actor.dashSpeed;
            speed.x = 0;
            speed.y = 0;
            externalForce = direction;
            dashCooldown = actor.maxDashCooldown;
            dashStaggerTime = actor.dashStagger;
            Invoke("StopDash", actor.dashDistance / actor.dashSpeed);
        }
    }

    /// <summary>
    /// Stops the dash after its duration has passed
    /// </summary>
    private void StopDash() {
        Dashing = false;
    }

    /// <summary>
    /// If the actor is standing on a platform, will ignore platforms briefly,
    /// otherwise it will just jump
    /// </summary>
    public void JumpDown() {
        if (CanMove()) {
            if (collisions.vHit &&
                LayerMask.GetMask(LayerMask.LayerToName(collisions.vHit.collider.gameObject.layer)) ==
                pConfig.owPlatformMask) {
                IgnorePlatforms();
            } else {
                Jump();
            }
        }
    }

    /// <summary>
    /// The actor will briefly ignore platforms so it can jump down through them
    /// </summary>
    private void IgnorePlatforms() {
        ignorePlatforms = owPlatformDelay;
    }

    /// <summary>
    /// The actor will briefly ignore ladders so it can jump or dash off of them
    /// </summary>
    private void IgnoreLadders() {
        ignoreLadders = ladderDelay;
    }

    /// <summary>
    /// Gives the actor its maximum extra jumps and air dashes
    /// </summary>
    public void ResetJumpsAndDashes() {
        extraJumps = actor.maxExtraJumps;
        airDashes = actor.maxAirDashes;
    }

    /// <summary>
    /// If not already climbing a ladder, tries to find one and attach t it
    /// </summary>
    /// <param name="direction"></param>
    public void ClimbLadder(float direction) {
        if (ignoreLadders > 0 || Dashing) {
            return;
        }
        float radius = myCollider.bounds.extents.x;
        Vector2 topOrigin = ((Vector2) myCollider.bounds.center) + Vector2.up * (myCollider.bounds.extents.y - radius);
        Vector2 bottomOrigin = ((Vector2) myCollider.bounds.center) + Vector2.down *
            (myCollider.bounds.extents.y + radius + skinWidth);
        if (!OnLadder && direction != 0 && Mathf.Abs(direction) > ladderClimbThreshold) {
            Collider2D hit = Physics2D.OverlapCircle(direction == -1 ? bottomOrigin : topOrigin,
                radius, pConfig.ladderMask);
            if (hit) {
                OnLadder = true;
                speed.x = 0;
                externalForce = Vector2.zero;
                ladderX = hit.transform.position.x;
            }
        }
        if (OnLadder) {
            float newX = Mathf.MoveTowards(transform.position.x, ladderX, 5f * Time.deltaTime);
            transform.Translate(newX - transform.position.x, 0, 0);
            ResetJumpsAndDashes();
            if (actor.ladderAccelerationTime > 0) {
                if (Mathf.Abs(speed.y) < actor.ladderSpeed) {
                    speed.y += direction * (1 / actor.ladderAccelerationTime) * actor.ladderSpeed * Time.deltaTime;
                }
            } else {
                speed.y = actor.ladderSpeed * direction;
            }
            if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(speed.y)) {
                if (actor.ladderDecelerationTime > 0) {
                    speed.y = Mathf.MoveTowards(speed.x, 0, (1 / actor.ladderDecelerationTime) *
                        actor.ladderSpeed * Time.deltaTime);
                } else {
                    speed.y = 0;
                }
            }
            if (Mathf.Abs(speed.y) > actor.ladderSpeed) {
                speed.y = Mathf.Min(speed.y, actor.ladderSpeed);
            }
            // checks ladder end
            Collider2D hit = Physics2D.OverlapCircle(topOrigin + Vector2.up * (speed.y * Time.deltaTime + radius),
                0, pConfig.ladderMask);
            if (!hit) {
                hit = Physics2D.OverlapCircle(bottomOrigin + Vector2.up * (speed.y * Time.deltaTime + radius),
                    0, pConfig.ladderMask);
                if (!hit) {
                    OnLadder = false;
                    if (speed.y > 0) {
                        speed.y = 0;
                    }
                }
            }
        }
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
    private void UpdateKnockback() {
        if (KnockedBack) {
            float directionX = Mathf.Sign(speed.x);
            float newHSpeed = Mathf.Abs(speed.x) - (pConfig.airFriction * Time.deltaTime);
            if (newHSpeed <= 0) {
                newHSpeed = 0;
                KnockedBack = false;
            }
            speed.x = newHSpeed * directionX;
        }
    }

    /// <summary>
    /// Updates dash related values
    /// </summary>
    private void UpdateDash() {
        if (dashCooldown > 0) {
            dashCooldown -= Time.deltaTime;
        }
        if (dashStaggerTime > 0 && !Dashing) {
            dashStaggerTime -= Time.deltaTime;
            externalForce *= Mathf.Max(1 - actor.staggerSpeedFalloff * Time.deltaTime, 0);
        }
    }

    /// <summary>
    /// Updates timers for different features
    /// </summary>
    private void UpdateTimers() {
        if (ignorePlatforms > 0) {
            ignorePlatforms -= Time.deltaTime;
        }
        if (ignoreLadders > 0) {
            ignoreLadders -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Checks if there are any knockbacks or status that stop all forms of movement,
    /// including jumping and dashing
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