using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEDAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ledPopSound;
    [SerializeField] private AudioClip ledHumSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
        if (audioSource == null || audioSource.clip != ledHumSound) return;

        if (audioSource.isPlaying) audioSource.Stop();
    }
}
