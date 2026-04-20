using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Sounds")]
    [SerializeField] private AudioClip componentAttachSound;
    [SerializeField] private AudioClip componentDetachSound;
    [SerializeField] private AudioClip componentSpawnSound;
    [SerializeField] private AudioClip componentDestroyedSound;



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

   

    public void PlayComponentSpawnSound()
    {
        audioSource?.PlayOneShot(componentSpawnSound, 2f);
    }

    public void PlayComponentDestroySound()
    {
        audioSource?.PlayOneShot(componentDestroyedSound, 1.5f);
    }

}
