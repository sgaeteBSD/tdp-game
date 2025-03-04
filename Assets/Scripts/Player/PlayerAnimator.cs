using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAnimator : MonoBehaviour
{

    private Animator animator;
    private SpriteRenderer sprr;
    private PlayerController pc;
    private FakeHeight fakeHeight;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        sprr = GetComponent<SpriteRenderer>();
        pc = GetComponentInParent<PlayerController>();
        fakeHeight = GetComponentInParent<FakeHeight>();
    }

    // Update is called once per frame
    void Update()
    {
        FlipX();
        Animate();
    }

    void FlipX()
    {
        if (pc.moveInput.x < 0)
        {
            sprr.flipX = true;
        }
        else if (pc.moveInput.x > 0)
        {
            sprr.flipX = false;
        }
    }

    private string[] animStates =
    {
        "Jump", "Stand", "Walk",
        "Fall", "Dash", "Roll",
        "FallDeath", "FallDeathFlipped",
        "Hurt"
    };
    private string prevState = "";
    void Animate()
    {
        float time = 0;
        foreach (var state in animStates)
        {
            if (IsCurrentAnimation(state) && state != prevState)
            {
                animator.Play(state, 0, time);
                prevState = state;
                CheckSpecialState(state);
            }
        }
    }

    bool IsCurrentAnimation(string state)
    {
        switch (state)
        {
            case "Stand":
                return pc.moveInput == Vector2.zero && fakeHeight.isGrounded && !pc.dashing && !pc.endDash && !pc.hasFallen && !pc.takingDamage;
            case "Jump":
                return !fakeHeight.isGrounded && fakeHeight.verticalVelocity >= 0 && !pc.dashing && !pc.endDash && !pc.hasFallen && !pc.takingDamage;
            case "Fall":
                return !fakeHeight.isGrounded && fakeHeight.verticalVelocity < 0 && !pc.dashing && !pc.endDash && !pc.hasFallen && !pc.takingDamage;
            case "Walk":
                return pc.moveInput != Vector2.zero && fakeHeight.isGrounded && !pc.dashing && !pc.endDash && !pc.hasFallen && !pc.takingDamage;
            case "Dash":
                return (pc.dashing || pc.endDash) && !fakeHeight.isGrounded && !pc.hasFallen && !pc.takingDamage;
            case "Roll":
                return (pc.dashing || pc.endDash) && fakeHeight.isGrounded && !pc.hasFallen && !pc.takingDamage;
            case "FallDeath":
                return pc.hasFallen && !sprr.flipX && !pc.takingDamage;
            case "FallDeathFlipped":
                return pc.hasFallen && sprr.flipX && !pc.takingDamage;
            case "Hurt":
                return pc.takingDamage;
            default:
                return false;
        }
    }

    void CheckSpecialState(string state)
    {
        if (state == "FallDeath" || state == "FallDeathFlipped")
        {
            StartCoroutine("FallDeath");
        }
    }

    private float length;
    private float timePlayed = 0;
    IEnumerator FallDeath()
    {
        GetComponentInParent<PlayerSFX>().Fall();
        yield return new WaitForEndOfFrame();
        sprr.sortingLayerName = "Falling";
        length = animator.GetCurrentAnimatorStateInfo(0).length;
        while (timePlayed < length || transform.localScale.x > 0f)
        {
            transform.localScale -= new Vector3(0.05f, 0.05f, 0);
            yield return new WaitForSeconds(0.05f);
            timePlayed += 0.05f;
        }
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(0.25f);
        sprr.sortingLayerName = "Default";
        pc.StartCoroutine("Respawn");
        animator.Play("Stand");
    }
}
