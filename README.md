# Unity 2D Platformer Controller

2D platformers are one of the first and most widely used game genres, dating back to 1980 and having been adapted and combined with various mechanics and clever twists throughout a large number of titles since then. 

There are some different of ways of implementing character movement and controls for 2D platformers, each with its own pros and cons. The Unity engine offers very complete 2D physics features that allow you to create realistic platformer mechanics easily. Sometimes, however, you might want a more precise and controlled input and movement, rather than a realistic one. This approach makes use of [raycasts](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html) and manual calculations to deal with movement and collisions of an actor in the 2D platformer world in a simple way, while also allowing you to easily change values to tweak how it behaves.

#### Features
* Smooth and precise movement
* Allows for multiple jumps
* Easily add animations
  
#### Planned Features
* Ladders/Ropes
* Jump-through platforms
* Moving platforms
* Slopes
* Ledge grabs
* Wall climbing and sliding


## Getting Started

To use this project you need Unity 2017.1 or higher. 

The main classes used in this project are the Actor, used mainly for attributes and other adjustable values, and the Controlled2D, which handles all the movement and collision algorithms.

#### Movement values
* **maxSpeed:** The maximum horizontal speed the actor can reach
* **accelerationTime:** How much time (in seconds) an actor will take to reach maximum horizontal speed. A value of 0 will allow the actor to reach maximum speed instantly.
* **decelerationTime:** How much time (in seconds) an actor will take to stop completely from maximum speed. A value of 0 will allow the actor to stop instantly.
* **maxExtraJumps:** How many times an actor can jump again without touching the ground.
* **extraJumps:** How many extra jumps the actor has currently.
* **jumpHeight:** How high (in units) the actor can jump.
* **jumpHoldScale:** How much the actor is affected by gravity while the jump button is held. This affects not only jump height but also falling speed while holding the button. A value of 1 will make holding the button have no effect.

## Authors

* [Akashenen](https://github.com/akashenen/)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

