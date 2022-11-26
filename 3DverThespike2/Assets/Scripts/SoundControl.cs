using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundControl : MonoBehaviour
{
    public AudioClip audioSpike;
    public AudioClip audioReceive;
    public AudioClip audioToss;
    public AudioClip audioNet;
    public AudioClip audioServe;
    public AudioClip audioLand;


    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        this.audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string sound) {
        switch(sound) {
            case "RECEIVE" : audioSource.clip = audioReceive; break;
            case "SPIKE" : audioSource.clip = audioSpike;  break;
            case "TOSS": audioSource.clip = audioToss;break;
            case "NET": audioSource.clip = audioNet;break;
            case "LAND": audioSource.clip = audioLand;break;
            case "SERVE": audioSource.clip = audioServe;break;
        }
        audioSource.Play();
    }
}

