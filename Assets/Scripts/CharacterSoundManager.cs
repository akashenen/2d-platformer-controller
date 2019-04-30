using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CharacterSoundManager : MonoBehaviour {

    public AudioClip jumpSound;

    private AudioSource source;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        source = GetComponent<AudioSource>();
    }

    public void PlayJumpSound(float volume = 1f) {
        source.PlayOneShot(jumpSound);
    }
}