using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    [SerializeField] private AudioClip[] sounds;

    private bool walking = false;
    private bool walkSfxPlaying = false;

    void Update()
    {
        if (walking && !walkSfxPlaying)
        {
            // StartCoroutine("Walking");
        }
    }
    
    public void Walk()
    {
        walking = true;
    }

    public void StopWalk()
    {
        walking = false;
        StopCoroutine("Walking");
        walkSfxPlaying = false;
    }

    IEnumerator Walking()
    {
        while (walking)
        {
            walkSfxPlaying = true;
            SFXManager.instance.PlaySFX(sounds[0], transform, 0.25f, true);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Jump()
    {
        SFXManager.instance.PlaySFX(sounds[1], transform, 2, true);
    }

    public void Roll()
    {
        SFXManager.instance.PlaySFX(sounds[2], transform, 0.5f, true);
    }

    public void Dash()
    {
        SFXManager.instance.PlaySFX(sounds[3], transform, 0.5f, true);
    }

    public void Fall()
    {
        SFXManager.instance.PlaySFX(sounds[4], transform, true);
    }

    public void Hurt()
    {
        SFXManager.instance.PlaySFX(sounds[5], transform, true);
    }

    public void Grab() 
    {
        SFXManager.instance.PlaySFX(sounds[6], transform, true);
    }

    public void Drop() 
    {
        SFXManager.instance.PlaySFX(sounds[7], transform, true);
    }

    public void Throw() 
    {
        SFXManager.instance.PlaySFX(sounds[8], transform, true);
    }
}
