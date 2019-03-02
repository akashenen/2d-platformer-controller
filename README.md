# Unity 2D Platformer Controller

2D platformers are one of the first and most widely used game genres, dating back to 1980 and having been adapted and combined with various mechanics and clever twists throughout a large number of titles since then. 

There are some different of ways of implementing character movement and controls for 2D platformers, each with its own pros and cons. The Unity engine offers very complete 2D physics features that allow you to create realistic platformer mechanics easily. Sometimes, however, you might want a more precise and controlled input and movement, rather than a realistic one. This approach makes use of [raycasts](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html) and manual calculations to deal with movement and collisions of an actor in the 2D platformer world in a simple way, while also allowing you to easily change values to tweak how it behaves.

![Demo Gif](https://github.com/akashenen/2d-platformer-controller/blob/master/Gifs/demo.gif)

### Features

* Smooth and precise movement
* Allows for multiple jumps
* Easily add animations
* Jump-through platforms
* Wall jumping and sliding
* Slopes
* Dashing and air dashing
* Ladders/Ropes
  
### Planned Features

* Moving platforms
* Ledge grabs
* Running
* Ducking/Crawling
* Crumbling platforms
* Jump pads

## Getting Started

To use this project you need Unity 2017.1 or higher. 

The main classes used in this project are the [Actor](https://github.com/akashenen/2d-platformer-controller/blob/master/Assets/Scripts/Actor.cs), used mainly for attributes and other adjustable values, and the [Controller2D](https://github.com/akashenen/2d-platformer-controller/blob/master/Assets/Scripts/Controller2D.cs), which handles all the movement and collision algorithms.

### Movement values

#### Movement

* **maxSpeed:** The maximum horizontal speed the actor can reach
* **accelerationTime:** How much time (in seconds) an actor will take to reach maximum horizontal speed. A value of 0 will allow the actor to reach maximum speed instantly
* **decelerationTime:** How much time (in seconds) an actor will take to stop completely from maximum speed. A value of 0 will allow the actor to stop instantly
* **canUseSlopes:** If your game doesn't have slopes, you can disable this option so the actor never checks or adjusts for slopes, saving some performance

#### Jumping

* **maxExtraJumps:** How many times an actor can jump again without touching the ground
* **jumpHeight:** How high (in units) the actor can jump.
* **jumpHoldScale:** How much the actor is affected by gravity while the jump button is held. This affects not only jump height but also falling speed while holding the button. A value of 1 will make holding the button have no effect
* **advancedAirControl:** If enabled, will allow you to set values for acceleration and deceleration while in the air, making it possible to have more stiff (or otherwise) controls while airborne. If disabled, will use the default acceleration and deceleration times for air control
* **airAccelerationTime:** Same as accelerationTime, but only applies if advancedAirControl is enabled and the actor is not on the ground. A higher value will make it harder to turn while in the air and will require the player to start running on the ground before being able to make long jumps
* **airDecelerationTime:** Same as decelerationTime, but only applies if advancedAirControl is enabled and the actor is not on the ground

#### Wall Sliding and Jumping

* **canWallSlide:** If enabled, the actor will be able to slide down walls
* **wallSlideVelocity:** Speed in which the actor will slide down walls if sliding is enabled
* **canWallJump:** If enabled, the actor will be able to jump from walls
* **wallJumpVelocity:** Horizonal velocity added to the actor when jumping from walls

#### Dashing

* **canDash:** If enabled, the actor will be able to dash
* **omnidirectionalDash:** If enabled, the actor will be able to dash in any direction, otherwise only horizontal dashes are allowed
* **dashDownSlopes:** If enabled, the actor will dash down slopes (like in Mega Man X games), otherwise it will maintain it's height when encountering a down slope
* **canJumpDuringDash:** If enabled, the actor will be able to jump during of the dash
* **jumpCancelStagger:** If enabled, jumping will cancel the stagger time after the dash, allowing it to keep the momentum
* **dashDistance:** Maximum distance traveled during the dash
* **dashSpeed:** The speed in which the actor dashes
* **dashStagger:** Duration of the stagger the actor suffers after dashing, a value of 0 will make dashes have no stagger and keep all momentum
* **staggerSpeedFalloff:** How much friction the actor will have during the stagger, causing it to lose speed (tweek this value along with dashStagger to make dashes feel good)
* **maxDashCooldown:** Time before the actor can dash again after dashing
* **maxAirDashes:** How many times the actor can dash while in the air, a value of 0 will make the actor unable to air dash

#### Ladders

* **ladderSpeed:** How fast the actor can move up and down slopes
* **ladderAccelerationTime:** How much time (in seconds) an actor will take to reach maximum speed on ladders. A value of 0 will allow the actor to reach maximum speed instantly
* **ladderDecelerationTime:** How much time (in seconds) an actor will take to stop completely from maximum speed on ladders. A value of 0 will allow the actor to stop instantly
* **ladderJumpHeight:** How high (in units) the actor can jump when holding a ladder
* **ladderJumpVelocity:** Horizonal velocity added to the actor when jumping from ladders

## Authors

* [Akashenen](https://github.com/akashenen/)
* This project is based on and expands upon [Sebastian Lague's 2D Platformer Tutorial](https://github.com/SebLague/2DPlatformer-Tutorial)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
