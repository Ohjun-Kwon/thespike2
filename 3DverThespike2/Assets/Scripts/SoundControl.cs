using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundControl : MonoBehaviour
{
    public AudioClip audioSpike;
    public AudioClip audioReceive2;
    public AudioClip audioSpike2;
    public AudioClip audioFoot1;
    public AudioClip audioFoot2;
    public AudioClip audioFoot3;
    public AudioClip audioFoot4;
    public AudioClip audioFoot5;

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
        int i;
        AudioClip audiof;
        switch(sound) {
            case "RECEIVE" : 
            i =UnityEngine.Random.Range(0,2);
            switch(i) {
                default : audiof = audioReceive; break;
                case 1: audiof = audioReceive2; break;
            }
            AudioSource.PlayClipAtPoint(audiof ,transform.position); 
            break;
            case "SPIKE" : 
            i =UnityEngine.Random.Range(0,2);
            switch(i) {
                default : audiof = audioSpike; break;
                case 1: audiof = audioSpike2; break;
            }
            AudioSource.PlayClipAtPoint(audiof ,transform.position); 
            break;

            case "TOSS": AudioSource.PlayClipAtPoint(audioToss ,transform.position); break;
            case "NET": AudioSource.PlayClipAtPoint(audioNet ,transform.position); break;
            case "LAND": AudioSource.PlayClipAtPoint(audioLand ,transform.position); break;
            case "SERVE": AudioSource.PlayClipAtPoint(audioServe ,transform.position); break;
            case "FOOT" :
            i =UnityEngine.Random.Range(0,5);
            switch(i) {
                default: audiof = audioFoot1;break;
                case 1: audiof = audioFoot2;break;
                case 2:audiof = audioFoot3;break;
                case 3:audiof = audioFoot4;break;
                case 4:audiof = audioFoot5;break;
            } 
            AudioSource.PlayClipAtPoint(audiof ,transform.position); 
            break;


        }
        audioSource.Play();
    }
}

