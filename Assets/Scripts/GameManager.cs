using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class that manages the game, must be present at every scene
/// </summary>
public class GameManager : Singleton<GameManager>
{

    protected GameManager() { } // guarantee this will be always a singleton only - can't use the constructor!

    public List<Actor> players = new List<Actor>();
    public List<Actor> enemies = new List<Actor>();

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {

    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {

    }
}
