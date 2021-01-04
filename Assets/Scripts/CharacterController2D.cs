using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a component used alongside CharacterData that controls all the movement and physics operations
/// </summary>
[RequireComponent(typeof(CharacterData))]
[RequireComponent(typeof(Animator))]
public class CharacterController2D : ObjectController2D {
    // Animation attributes and names
    private static readonly string ANIMATION_H_SPEED = "hSpeed";
    private static readonly string ANIMATION_V_SPEED = "vSpeed";
    private static readonly string ANIMATION_EX_SPEED = "exSpeed";
    private static readonly string ANIMATION_JUMP = "jump";
    private static readonly string ANIMATION_GROUNDED = "grounded";
    private static readonly string ANIMATION_DASHING = "dashing";
    private static readonly string ANIMATION_WALL = "onWall";
    private static readonly string ANIMATION_FACING = "facingRight";
    private static readonly string ANIMATION_LADDER = "onLadder";
    private static readonly string ANIMATION_INVULNERABLE = "invulnerable";

    // Other Componenents
    private CharacterData cData;
    private Animator animator;
    private CharacterSoundManager soundManager;
    [SerializeField]
    private SpriteRenderer visual;

    // Physics properties
    private float ignoreLaddersTime = 0;
    [SerializeField]
    private int extraJumps = 0;
    [SerializeField]
    private int airDashes = 0;
    private float dashCooldown = 0;
    private float airStaggerTime = 0;
    private float stunTime = 0;
    private float invulnerableTime = 0;
    private float ladderX = 0;

    // Public propoerties
    public bool OnLadder { get; set; }
    public bool Stunned => stunTime > 0;
    public bool Invulnerable => invulnerableTime > 0;
    public bool Immobile { get; set; }
    public bool Dashing { get; set; }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    public override void Start() {
        cData = GetComponent<CharacterData>();
        animator = GetComponent<Animator>();
        soundManager = GetComponent<CharacterSoundManager>();
        OnLadder = false;
        Dashing = false;
        base.Start();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    public override void FixedUpdate() {
        UpdateTimers();
        UpdateDash();
        UpdateAirStagger();
        collisions.Reset();
        Move((TotalSpeed) * Time.fixedDeltaTime);
        PostMove();
        SetAnimations();
    }

    /*-------------------------*/
    /*--------MOVEMENT---------*/
    /*-------------------------*/

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
            if (deltaMove.y <= 0 && cData.canUseSlopes) {
                if (collisions.onSlope) {
                    if (collisions.groundDirection == xDir) {
                        if ((!Dashing && airStaggerTime <= 0) || cData.dashDownSlopes) {
                            DescendSlope(ref deltaMove);
                        }
                    } else {
                        ClimbSlope(ref deltaMove);
                    }
                }
            }
            HorizontalCollisions(ref deltaMove);
        }
        if (collisions.hHit && cData.canWallSlide && TotalSpeed.y <= 0) {
            externalForce.y = 0;
            speed.y = -cData.wallSlideSpeed;
        }
        if (collisions.onSlope && collisions.groundAngle >= minWallAngle &&
            collisions.groundDirection != xDir && speed.y < 0) {
            float sin = Mathf.Sin(collisions.groundAngle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(collisions.groundAngle * Mathf.Deg2Rad);
            deltaMove.x = cos * cData.wallSlideSpeed * Time.fixedDeltaTime * collisions.groundDirection;
            deltaMove.y = sin * -cData.wallSlideSpeed * Time.fixedDeltaTime;
            speed.y = -cData.wallSlideSpeed;
            speed.x = 0;
            Vector2 origin = collisions.groundDirection == -1 ? raycastOrigins.bottomRight : raycastOrigins.bottomRight;
            collisions.hHit = Physics2D.Raycast(origin, Vector2.left * collisions.groundDirection,
                1f, collisionMask);
        }
        if (collisions.onGround && deltaMove.x != 0 && speed.y <= 0) {
            HandleSlopeChange(ref deltaMove);
        }
        if (deltaMove.y > 0 || (deltaMove.y < 0 && (!collisions.onSlope || deltaMove.x == 0))) {
            VerticalCollisions(ref deltaMove);
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
        gameObject.layer = layer;
        return deltaMove;
    }

    /// <summary>
    /// Checks for collisions in the vertical axis and adjust the speed accordingly to stop at the 
    /// collided object.
    /// </summary>
    /// <param name="deltaMove">The current object deltaMove used for the raycast lenght</param>
    protected override void VerticalCollisions(ref Vector2 deltaMove) {
        if (OnLadder) {
            collisions.Reset();
            Vector2 origin = myCollider.bounds.center + Vector3.up *
                (myCollider.bounds.extents.y * Mathf.Sign(deltaMove.y) + deltaMove.y);
            Collider2D hit = Physics2D.OverlapCircle(origin, 0, collisionMask);
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
                rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            // for one way platforms
            if (ignorePlatformsTime <= 0 && directionY < 0 && !hit) {
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down,
                    rayLength, pConfig.owPlatformMask);
                foreach (RaycastHit2D h in hits) {
                    if (h.distance > 0) {
                        hit = h;
                        continue;
                    }
                }
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
    /// Checks for angle changes on the ground, preventing the character from briefly passing through ground 
    /// and losing deltaMove or leaving the ground and floating (lots of trigonometry)
    /// </summary>
    /// <param name="deltaMove">The current character deltaMove</param>
    protected override void HandleSlopeChange(ref Vector2 deltaMove) {
        if (deltaMove.y <= 0 && (Dashing || airStaggerTime > 0) && !cData.dashDownSlopes) {
            return;
        } else {
            base.HandleSlopeChange(ref deltaMove);
        }
    }

    /// <summary>
    /// Updates the character's vertical speed according to gravity, gravity scale and other properties
    /// </summary>
    protected override void UpdateGravity() {
        if (!OnLadder && !Dashing && airStaggerTime <= 0) {
            base.UpdateGravity();
        }
    }

    /// <summary>
    /// Reduces the external force over time according to the air or ground frictions
    /// </summary>
    protected override void UpdateExternalForce() {
        if (!Dashing && airStaggerTime <= 0) {
            base.UpdateExternalForce();
        }
    }

    /// <summary>
    /// Sets the character's external force to the specified amount
    /// </summary>
    /// <param name="force">Force to be set</param>
    public override void SetForce(Vector2 force) {
        base.SetForce(force);
        // cancels dash
        Dashing = false;
        airStaggerTime = 0;
    }

    /// <summary>
    /// Sets the character's air stagger duration, causing it not to be affected by gravity or inputs and 
    /// osing speed over time
    /// </summary>
    /// <param name="duration">Air stagger duration</param>
    public void SetAirStagger(float duration) {
        airStaggerTime = duration;
    }

    /// <summary>
    /// Adds a knockback force to the character into an specific direction, while also disabling their movement
    /// for the specified duration
    /// </summary>
    /// <param name="force">Direction and magnitude of the force applied</param>
    /// <param name="stunDuration">How much time the character will be stunned</param>
    public void Knockback(Vector2 force, float stunDuration) {
        if (!Invulnerable) {
            Dashing = false;
            airStaggerTime = 0;
            externalForce = Vector2.zero;
            speed = Vector2.zero;
            stunTime = stunDuration;
            ApplyForce(force);
        }
    }

    /// <summary>
    /// Makes the character invulnerable for the specified duration, preventing further damage or knockbacks
    /// </summary>
    /// <param name="duration">Invulnerability duration</param>
    public void setInvunerable(float duration) {
        invulnerableTime = duration;
    }

    /// <summary>
    /// Updates the character's animator with the movement and collision values
    /// </summary>
    private void SetAnimations() {
        if(TotalSpeed.x != 0) {
            FacingRight = TotalSpeed.x > 0; 
            if (visual) visual.flipX = !FacingRight;
        }
        animator.SetFloat(ANIMATION_H_SPEED, speed.x);
        animator.SetFloat(ANIMATION_V_SPEED, TotalSpeed.y);
        animator.SetFloat(ANIMATION_EX_SPEED, externalForce.x);
        animator.SetBool(ANIMATION_GROUNDED, collisions.onGround);
        animator.SetBool(ANIMATION_DASHING, Dashing);
        animator.SetBool(ANIMATION_WALL, collisions.hHit);
        animator.SetBool(ANIMATION_FACING, FacingRight);
        animator.SetBool(ANIMATION_LADDER, OnLadder);
        animator.SetBool(ANIMATION_INVULNERABLE, Invulnerable);
    }

    /// <summary>
    /// Tries to move the character horizontally based on it's current movespeed and input pressure 
    /// while checking for movement impairments
    /// </summary>
    /// <param name="direction">-1 to 1; negative values = left; positive values = right</param>
    public void Walk(float direction) {
        if (collisions.onSlope && collisions.groundAngle > maxSlopeAngle && collisions.groundAngle < minWallAngle) {
            direction = 0;
        }
        if (CanMove() && !Dashing && airStaggerTime <= 0) {
            if (OnLadder) {
                if(direction != 0) {
                    FacingRight = direction > 0; 
                    if (visual) visual.flipX = !FacingRight;
                }
                return;
            }
            float acc = 0f;
            float dec = 0f;
            if (cData.advancedAirControl && !collisions.below) {
                acc = cData.airAccelerationTime;
                dec = cData.airDecelerationTime;
            } else {
                acc = cData.accelerationTime;
                dec = cData.decelerationTime;
            }
            if (acc > 0) {
                if(externalForce.x != 0 && Mathf.Sign(externalForce.x) != Mathf.Sign(direction)) {
                    externalForce.x += direction * (1 / acc) * cData.maxSpeed * Time.fixedDeltaTime;
                } else {
                    if (Mathf.Abs(speed.x) < cData.maxSpeed) {
                        speed.x += direction * (1 / acc) * cData.maxSpeed * Time.fixedDeltaTime;
                        speed.x = Mathf.Min(Mathf.Abs(speed.x), cData.maxSpeed * Mathf.Abs(direction)) *
                            Mathf.Sign(speed.x);
                    }
                }
                
            } else {
                speed.x = cData.maxSpeed * direction;
            }
            if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(speed.x)) {
                if (dec > 0) {
                    speed.x = Mathf.MoveTowards(speed.x, 0, (1 / dec) * cData.maxSpeed * Time.fixedDeltaTime);
                } else {
                    speed.x = 0;
                }
            }
        }
    }

    

    /// <summary>
    ///  Makes the character jump if possible
    /// </summary>
    public void Jump() {
        if (CanMove() && (!Dashing || cData.canJumpDuringDash)) {
            if (collisions.onGround || extraJumps > 0 || (cData.canWallJump && collisions.hHit)) {
                // air jump
                if (!collisions.onGround && !OnLadder) {
                    extraJumps--;
                    externalForce = Vector2.zero;
                }
                float height = cData.maxJumpHeight;
                if (OnLadder) {
                    Vector2 origin = myCollider.bounds.center + Vector3.up * myCollider.bounds.extents.y;
                    Collider2D hit = Physics2D.OverlapCircle(origin, 0, collisionMask);
                    if (hit) {
                        return;
                    }
                    origin = myCollider.bounds.center + Vector3.down * myCollider.bounds.extents.y;
                    hit = Physics2D.OverlapCircle(origin, 0, collisionMask);
                    if (hit) {
                        return;
                    }
                    height = cData.ladderJumpHeight;
                    externalForce.x += cData.ladderJumpSpeed * (FacingRight ? 1 : -1);
                    OnLadder = false;
                    IgnoreLadders();
                    ResetJumpsAndDashes();
                }
                speed.y = Mathf.Sqrt(-2 * pConfig.gravity * height);
                externalForce.y = 0;
                animator.SetTrigger(ANIMATION_JUMP);
                if (cData.jumpCancelStagger) {
                    airStaggerTime = 0;
                }
                // wall jump
                if (cData.canWallJump && collisions.hHit && !collisions.below) {
                    externalForce.x += collisions.left ? cData.wallJumpSpeed : -cData.wallJumpSpeed;
                    ResetJumpsAndDashes();
                }
                // slope sliding jump
                if (collisions.onSlope && collisions.groundAngle > maxSlopeAngle &&
                    collisions.groundAngle < minWallAngle) {
                    speed.x = cData.maxSpeed * collisions.groundDirection;
                }
                ignorePlatformsTime = 0;
                if (soundManager) {
                    soundManager.PlayJumpSound();
                }
            }
        }
    }

    /// <summary>
    /// Ends the jump ealier by setting the vertical speed to the minimum jump speed if it's higher
    /// </summary>
    public void EndJump() {
        float yMove = Mathf.Sqrt(-2 * pConfig.gravity * cData.minJumpHeight);
        if (speed.y > yMove) {
            speed.y = yMove;
        }
    }

    /// <summary>
    /// Makes the character dash in the specified direction if possible.
    /// If omnidirectional dash is disabled, will only dash in the horizontal axis
    /// </summary>
    /// <param name="direction">The desired direction of the dash</param>
    public void Dash(Vector2 direction) {
        if (CanMove() && cData.canDash && dashCooldown <= 0) {
            if (OnLadder) {
                Vector2 origin = myCollider.bounds.center + Vector3.up * myCollider.bounds.extents.y;
                Collider2D hit = Physics2D.OverlapCircle(origin, 0, collisionMask);
                if (hit) {
                    return;
                }
                origin = myCollider.bounds.center + Vector3.down * myCollider.bounds.extents.y;
                hit = Physics2D.OverlapCircle(origin, 0, collisionMask);
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
            if (!cData.omnidirectionalDash) {
                direction = Vector2.right * Mathf.Sign(direction.x);
            }
            direction = direction.normalized * cData.dashSpeed;
            speed.x = 0;
            speed.y = 0;
            externalForce = direction;
            dashCooldown = cData.maxDashCooldown;
            airStaggerTime = cData.dashStagger;
            Invoke("StopDash", cData.dashDistance / cData.dashSpeed);
        }
    }

    /// <summary>
    /// Stops the dash after its duration has passed
    /// </summary>
    private void StopDash() {
        Dashing = false;
    }

    /// <summary>
    /// If the character is standing on a platform, will ignore platforms briefly,
    /// otherwise it will just jump
    /// </summary>
    public void JumpDown() {
        if (CanMove()) {
            if (collisions.vHit && pConfig.owPlatformMask ==
                (pConfig.owPlatformMask | (1 << collisions.vHit.collider.gameObject.layer))) {
                IgnorePlatforms();
            } else {
                Jump();
            }
        }
    }

    /// <summary>
    /// The character will briefly ignore platforms so it can jump down through them
    /// </summary>
    private void IgnorePlatforms() {
        ignorePlatformsTime = owPlatformDelay;
    }

    /// <summary>
    /// The character will briefly ignore ladders so it can jump or dash off of them
    /// </summary>
    private void IgnoreLadders() {
        ignoreLaddersTime = ladderDelay;
    }

    /// <summary>
    /// Gives the character its maximum extra jumps and air dashes
    /// </summary>
    public void ResetJumpsAndDashes() {
        extraJumps = cData.maxExtraJumps;
        airDashes = cData.maxAirDashes;
    }

    /// <summary>
    /// If not already climbing a ladder, tries to find one and attach to it
    /// </summary>
    /// <param name="direction"></param>
    public void ClimbLadder(float direction) {
        if (ignoreLaddersTime > 0 || Dashing) {
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
            float newX = Mathf.MoveTowards(transform.position.x, ladderX, 5f * Time.fixedDeltaTime);
            transform.Translate(newX - transform.position.x, 0, 0);
            ResetJumpsAndDashes();
            if (cData.ladderAccelerationTime > 0) {
                if (Mathf.Abs(speed.y) < cData.ladderSpeed) {
                    speed.y += direction * (1 / cData.ladderAccelerationTime) * cData.ladderSpeed * Time.fixedDeltaTime;
                }
            } else {
                speed.y = cData.ladderSpeed * direction;
            }
            if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(speed.y)) {
                if (cData.ladderDecelerationTime > 0) {
                    speed.y = Mathf.MoveTowards(speed.x, 0, (1 / cData.ladderDecelerationTime) *
                        cData.ladderSpeed * Time.fixedDeltaTime);
                } else {
                    speed.y = 0;
                }
            }
            if (Mathf.Abs(speed.y) > cData.ladderSpeed) {
                speed.y = Mathf.Min(speed.y, cData.ladderSpeed);
            }
            // checks ladder end
            Collider2D hit = Physics2D.OverlapCircle(topOrigin + Vector2.up * (speed.y * Time.fixedDeltaTime + radius),
                0, pConfig.ladderMask);
            if (!hit) {
                hit = Physics2D.OverlapCircle(bottomOrigin + Vector2.up *
                    (speed.y * Time.fixedDeltaTime + radius - skinWidth * 3), skinWidth, pConfig.ladderMask);
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
    /// Updates dash related values
    /// </summary>
    private void UpdateDash() {
        if (dashCooldown > 0) {
            dashCooldown -= Time.fixedDeltaTime;
        }
    }

    private void UpdateAirStagger() {
        if (airStaggerTime > 0 && !Dashing) {
            airStaggerTime -= Time.fixedDeltaTime;
            externalForce = Vector2.MoveTowards(externalForce, Vector2.zero,
                pConfig.staggerSpeedFalloff * Time.fixedDeltaTime);
            speed = Vector2.MoveTowards(speed, Vector2.zero,
                pConfig.staggerSpeedFalloff * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Updates timers for different features
    /// </summary>
    private void UpdateTimers() {
        if (ignorePlatformsTime > 0) {
            ignorePlatformsTime -= Time.fixedDeltaTime;
        }
        if (ignoreLaddersTime > 0) {
            ignoreLaddersTime -= Time.fixedDeltaTime;
        }
        if (stunTime > 0) {
            stunTime -= Time.fixedDeltaTime;
        }
        if (invulnerableTime > 0) {
            invulnerableTime -= Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Checks if there are any stuns or other status that stop all forms of movement,
    /// including jumping and dashing
    /// </summary>
    /// <returns>Whether the character can move or not</returns>
    public bool CanMove() {
        return (!Stunned && !Immobile);
    }
}