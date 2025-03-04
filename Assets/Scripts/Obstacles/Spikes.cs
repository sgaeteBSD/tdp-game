using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spikes : MonoBehaviour
{

    private BoxCollider2D bc;
    
    private bool characterGrounded = true;
    private bool spikesOut = false;

    private SpriteRenderer spikeSprite;
    private SquashNStretch sns;
    
    public float startTime = 0f;
    public float length = 1.5f;

    [Header("Audio")]
    public AudioClip[] sounds;

    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        // animator = GetComponent<Animator>();
        // animator.Play("SpikesIn");
        SpriteRenderer[] getSprite = GetComponentsInChildren<SpriteRenderer>().Skip(1).ToArray();
        spikeSprite = getSprite[0];
        sns = GetComponentInChildren<SquashNStretch>();
        bc.enabled = false;
        StartCoroutine("InAndOut");
    }

    void Update()
    {
        characterGrounded = GameObject.FindGameObjectWithTag("Player").GetComponent<FakeHeight>().isGrounded;
        bc.enabled = characterGrounded && spikesOut;
    }

    float waitSpikeIn = 0.1f;
    float waitSpikeOut = 0.25f;
    
    IEnumerator InAndOut()
    {
        yield return new WaitForSeconds(startTime);
        while (true)
        {
            yield return new WaitForSeconds(length - waitSpikeIn);
            SFXManager.instance.PlaySFX(sounds[0], transform, 0.3f, false);
            yield return new WaitForSeconds(waitSpikeIn);
            spikesOut = true;
            spikeSprite.enabled = true;
            sns.PlaySquashAndStretch();
            // animator.Play("SpikesOut");
            yield return new WaitForSeconds(length - waitSpikeOut);
            SFXManager.instance.PlaySFX(sounds[1], transform, 0.15f, true);
            yield return new WaitForSeconds(waitSpikeOut);
            spikesOut = false;
            // animator.Play("SpikesIn");
            spikeSprite.enabled = false;
        }
    }
}
