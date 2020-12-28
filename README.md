# Unity 2D Platformer Controller

2D platformers are one of the first and most widely used game genres, dating back to 1980 and having been adapted and combined with various mechanics and clever twists throughout a large number of titles since then. 

There are some different of ways of implementing character movement and controls for 2D platformers, each with its own pros and cons. The Unity engine offers very complete 2D physics features that allow you to create realistic platformer mechanics easily. Sometimes, however, you might want a more precise and controlled input and movement, rather than a realistic one. This approach makes use of [raycasts](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html) and manual calculations to deal with movement and collisions of an character in the 2D platformer world in a simple way, while also allowing you to easily change values to tweak how it behaves.

![Demo Gif](https://github.com/akashenen/2d-platformer-controller/blob/master/Gifs/demo2.gif)

### Features

* Smooth and precise movement
* Allows for multiple jumps
* Easily add animations
* One way platforms
* Wall jumping and sliding
* Slopes
* Dashing and air dashing
* Ladders/Ropes
* Moving platforms
* Crumbling platforms
* Jump pads
* Checkpoint system
* Environment hazards
* Pick up objects
* Pressure plates/levers/other triggers
  
### Planned Features

* Ledge grabs
* Running
* Ducking/Crawling
* Basic combat templates

## <span style="color:red">New</span>

* Updated to use Unity 2019.4
* Fixed animation errors and added sprite example, art by [rvros](https://rvros.itch.io/animated-pixel-hero)


### Known Issues

* Slopes are still not perfect, specially "slide-only" angles. I'd stick to using only climbable angles (configurable) to avoid problems.
* Throwable objects can become stuck to walls if inside them when released.

## Getting Started

The base classes of this project can work Unity 2017.1 or higher, but 2019.4 or higher is recommended.

The main classes used in this project are the [CharacterData](https://github.com/akashenen/2d-platformer-controller/blob/master/Assets/Scripts/CharacterData.cs), used mainly for attributes and other adjustable values, and the [CharacterController2D](https://github.com/akashenen/2d-platformer-controller/blob/master/Assets/Scripts/CharacterController2D.cs), which handles all the movement and collision algorithms.

### Character Properties

#### Movement

* **maxSpeed:** The maximum horizontal speed the character can reach
* **accelerationTime:** How much time (in seconds) an character will take to reach maximum horizontal speed. A value of 0 will allow the character to reach maximum speed instantly
* **decelerationTime:** How much time (in seconds) an character will take to stop completely from maximum speed. A value of 0 will allow the character to stop instantly
* **canUseSlopes:** If your game doesn't have slopes, you can disable this option so the character never checks or adjusts for slopes, saving some performance

#### Jumping

* **maxExtraJumps:** How many times an character can jump again without touching the ground
* **maxJumpHeight:** How high (in units) the character can jump while holding the jump button
* **minJumpHeight:** How high (in units) the character will keep jumping after releasing the jump button, ending the jump earlier
* **advancedAirControl:** If enabled, will allow you to set values for acceleration and deceleration while in the air, making it possible to have more stiff (or otherwise) controls while airborne. If disabled, will use the default acceleration and deceleration times for air control
* **airAccelerationTime:** Same as accelerationTime, but only applies if advancedAirControl is enabled and the character is not on the ground. A higher value will make it harder to turn while in the air and will require the player to start running on the ground before being able to make long jumps
* **airDecelerationTime:** Same as decelerationTime, but only applies if advancedAirControl is enabled and the character is not on the ground

#### Wall Sliding and Jumping

* **canWallSlide:** If enabled, the character will be able to slide down walls
* **wallSlideSpeed:** Speed in which the character will slide down walls if sliding is enabled
* **canWallJump:** If enabled, the character will be able to jump from walls
* **wallJumpSpeed:** Horizonal speed added to the character when jumping from walls

#### Dashing

* **canDash:** If enabled, the character will be able to dash
* **omnidirectionalDash:** If enabled, the character will be able to dash in any direction, otherwise only horizontal dashes are allowed
* **dashDownSlopes:** If enabled, the character will dash down slopes (like in Mega Man X games), otherwise it will maintain it's height when encountering a down slope
* **canJumpDuringDash:** If enabled, the character will be able to jump during of the dash
* **jumpCancelStagger:** If enabled, jumping will cancel the stagger time after the dash, allowing it to keep the momentum
* **dashDistance:** Maximum distance traveled during the dash
* **dashSpeed:** The speed in which the character dashes
* **dashStagger:** Duration of the stagger the character suffers after dashing, a value of 0 will make dashes have no stagger and keep all momentum
* **staggerSpeedFalloff:** How much friction the character will have during the stagger, causing it to lose speed (tweek this value along with dashStagger to make dashes feel good)
* **maxDashCooldown:** Time before the character can dash again after dashing
* **maxAirDashes:** How many times the character can dash while in the air, a value of 0 will make the character unable to air dash

#### Ladders

* **ladderSpeed:** How fast the character can move up and down slopes
* **ladderAccelerationTime:** How much time (in seconds) an character will take to reach maximum speed on ladders. A value of 0 will allow the character to reach its maximum speed instantly
* **ladderDecelerationTime:** How much time (in seconds) an character will take to stop completely from maximum speed on ladders. A value of 0 will allow the character to stop instantly
* **ladderJumpHeight:** How high (in units) the character can jump when holding a ladder
* **ladderJumpSpeed:** Horizonal speed added to the character when jumping from ladders

### Platform Properties

* **currentWaypoint:** For moving platforms, set this as the first waypoint the platform will move towards, after arriving at that waypoint, the platform will automatically target the next one on the line (which needs to be configured in each waypoint object)
* **maxSpeed:** The maximum speed in which the platform can move
* **accelerationDistance:**  The distance (in units) the platform will cover while accelerating to maximum speed. A value of 0 will make the platform reach its maximum speed instantly
* **decelerationDistance:** The distance (in units) the platform will cover while decelerating when arriving at the current waypoint. A value of 0 will make the platform reach the waypoint at full speed
* **waitTime:** Hou much time (in seconds) the platform will wait at each waypoint before heading on to the next one
* **crumbleTime:** How much time it takes for a platform to crumble and lose its hitbox. A value of 0 will make the platform never crumble
* **restoreTime:** How much time it takes for the platform to be restored after it crumbles. A value of 0 will cause the platform not to be restored automatically, but it can be restored by calling the method Restore() (useful if you want it to have other conditions to be restored)
* **onlyPlayerCrumble:** If enabled, the platform will only crumble if a player touches it, causing it to remain active if touched by enemies or other objects

### Physics Config

* **gravity:** How much vertical acceleration will be applied to an character each second (should be a negative value)
* **airFriction:** How much speed per second an character will lose while in the air (doesn't affect player movement, only external forces like knockbacks and jump pads)
* **groundFriction:** How much speed per second an character will lose while touching the ground (doesn't affect player movement, only external forces like knockbacks and jump pads)

## Authors

* [Akashenen](https://github.com/akashenen/)
* This project is based on and expands upon [Sebastian Lague's 2D Platformer Tutorial](https://github.com/SebLague/2DPlatformer-Tutorial)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
