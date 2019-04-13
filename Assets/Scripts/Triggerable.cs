using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerable : MonoBehaviour {

    public static readonly string ANIMATION_ACTIVE = "active";

    public TriggerObject trigger;

    private Animator animator;

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        if (animator && trigger) {
            animator.SetBool(ANIMATION_ACTIVE, trigger.Active);
        }
    }
}