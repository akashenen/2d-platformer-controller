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

## Authors

* [Akashenen](https://github.com/akashenen/)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

