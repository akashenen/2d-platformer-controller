using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CameraController : MonoBehaviour {

    public float fadeOutDuration = 0.5f;
    public float fadeInDuration = 0.5f;

    private Animator animator;
    private static readonly string ANIMATION_BLACK = "black";
    private static readonly string ANIMATION_IN_SPEED = "fadeInSpeed";
    private static readonly string ANIMATION_OUT_SPEED = "fadeOutSpeed";

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Fades the screen from black to visible
    /// </summary>
    public void FadeIn() {
        animator.SetBool(ANIMATION_BLACK, false);
        float speed = fadeInDuration > 0f ? 1f / fadeInDuration : 1f;
        animator.SetFloat(ANIMATION_IN_SPEED, speed);
    }

    /// <summary>
    /// Fades the screen from visible to black
    /// </summary>
    public void FadeOut() {
        animator.SetBool(ANIMATION_BLACK, true);
        float speed = fadeOutDuration > 0f ? 1f / fadeOutDuration : 1f;
        animator.SetFloat(ANIMATION_OUT_SPEED, speed);
    }
}