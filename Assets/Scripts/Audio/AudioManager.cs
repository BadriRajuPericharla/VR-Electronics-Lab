using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Sounds")]
    [SerializeField] private AudioClip componentAttachSound;
    [SerializeField] private AudioClip componentDetachSound;
    [SerializeField] private AudioClip ledPopSound;
    [SerializeField] private AudioClip ledHumSound;


    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAttachSound()
    {
        audioSource?.PlayOneShot(componentAttachSound, 0.1f);
    }

    public void PlayDetachSound()
    {
        audioSource?.PlayOneShot(componentDetachSound, 0.1f);
    }

    public void PlayLEDBlownSound()
    {
        audioSource?.PlayOneShot(ledPopSound);
    }

    public void PlayLEDHumSound()
    {
        audioSource.clip = ledHumSound;
        audioSource.loop = true;
        audioSource.Play();
    }


    public void StopLEDHumSound()
    {
        if (audioSource == null && audioSource.clip != ledHumSound) return;

        if(audioSource.isPlaying) audioSource.Stop();
    }
}
