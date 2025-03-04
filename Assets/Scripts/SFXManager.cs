using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    [SerializeField] private AudioSource sfxObject;
    [SerializeField] public float volume;

    [SerializeField] private AudioSource musicObject;
    
    
    private List<AudioSource> sounds;
    
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            // GameObject music = GameObject.FindWithTag("Music");
            // if (music != null)
            // {
            //     musicObject = music.GetComponent<AudioSource>();
            // }
            sounds = new List<AudioSource>();
        }
    }

    List<AudioSource> soundsToRemove = new List<AudioSource>();

    void Update()
    {
        sounds.RemoveAll(s => s == null); 
    }

    public AudioSource PlaySFX(AudioClip audioClip, Transform pos, float vol, bool randomPitch)
    {
        AudioSource audioSource;
        audioSource = Instantiate(sfxObject, pos.position, Quaternion.identity);
        sounds.Add(audioSource);
        audioSource.clip = audioClip;
        audioSource.volume = vol;
        if (randomPitch)
        {
            audioSource.pitch = Eerp(0.8f, 1.2f, Random.value);
            // audioSource.pitch = Random.Range(0.8f, 1.2f);
        }
        audioSource.Play();
        if(audioSource.gameObject != null) 
        {
            float clipLength = audioSource.clip.length; 
            Destroy(audioSource.gameObject, audioSource.clip.length);
        }
        return audioSource;
    }

    public void PlaySFX(AudioClip audioClip, Transform pos, bool randomPitch)
    {
        PlaySFX(audioClip, pos, volume, randomPitch);
    }

    public void StopAll()
    {
        foreach (var sound in sounds)
        {
            sound.Stop();
        }
        musicObject.Stop();
    }

    public void StopMusic()
    {
        musicObject.Stop();
    }

    // AudioSource RandomizePitch(AudioSource audioSource)
    // {
    //     audioSource.pitch = Eerp(0.8f, 1.2f, Random.Range(0, 1));
    //     return audioSource;
    // }

    // Allows for proper random pitching both higher and lower
    static float Eerp( float a, float b, float t ) {
        return a * MathF.Exp( t * MathF.Log( b / a ) );
    }
    
}